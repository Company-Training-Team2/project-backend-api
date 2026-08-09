using EventHub.Application.DTOs.Auth;
using EventHub.Application.Helpers;
using EventHub.Application.Interfaces;
using EventHub.Domain.Constants;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventHub.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IMfaService _mfaService;
    private readonly JwtHelper _jwtHelper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IMfaService mfaService,
        JwtHelper jwtHelper,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _mfaService = mfaService;
        _jwtHelper = jwtHelper;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════
    // Register
    // ═══════════════════════════════════════════════════════════
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (request.Role == UserRole.Admin)
            throw new InvalidOperationException(AuthConstants.AdminRegistrationForbiddenMessage);

        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            throw new InvalidOperationException(AuthConstants.EmailAlreadyRegisteredMessage);

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            Role = request.Role,
            IsEmailVerified = false,
            IsActive = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        // ── OTP email verification ─────────────────────────────────────────────
        var otpCode = GenerateSixDigitCode();
        user.EmailVerificationCode = otpCode;
        user.EmailVerificationExpiry = DateTime.UtcNow.AddMinutes(AuthConstants.EmailOtpExpiryMinutes);
        await _userManager.UpdateAsync(user);

        // Best-effort: the account + OTP code are already persisted above, so a
        // flaky SMTP server must not fail the whole registration — that would
        // leave the caller with an unverifiable, unrecoverable "Email already
        // registered" account. The user can still request a fresh code once
        // "resend OTP" exists; for now they just see the generic success message.
        try
        {
            await _emailService.SendVerificationOtpAsync(user.Email, otpCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send verification OTP email to {Email}", user.Email);
        }

        // ── Create role profile ────────────────────────────────────────────────
        if (request.Role == UserRole.Customer)
        {
            await _unitOfWork.Repository<CustomerProfile>().AddAsync(new CustomerProfile
            {
                UserId = user.Id,
                FullName = request.FullName ?? string.Empty,
                PhoneNumber = request.PhoneNumber,
                City = request.City,
                CreatedAt = DateTime.UtcNow
            });
        }
        else if (request.Role == UserRole.Vendor)
        {
            await _unitOfWork.Repository<VendorProfile>().AddAsync(new VendorProfile
            {
                UserId = user.Id,
                BusinessName = request.BusinessName ?? string.Empty,
                BioDescription = request.BioDescription ?? string.Empty,
                ApprovalStatus = ApprovalStatus.Pending,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.SaveChangesAsync();

        return new AuthResponse
        {
            Message = AuthConstants.RegistrationSuccessMessage
        };
    }

    // ═══════════════════════════════════════════════════════════
    // Email OTP Verification
    // ═══════════════════════════════════════════════════════════
    public async Task<bool> VerifyEmailOtpAsync(VerifyEmailOtpRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null) return false;
        if (user.EmailVerificationCode != request.Code) return false;
        if (user.EmailVerificationExpiry < DateTime.UtcNow) return false;

        user.IsEmailVerified = true;
        user.EmailConfirmed = true;
        user.EmailVerificationCode = null;
        user.EmailVerificationExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);
        return true;
    }

    // ═══════════════════════════════════════════════════════════
    // Google Login
    // ═══════════════════════════════════════════════════════════
    public async Task<AuthResponse> GoogleAuthAsync(GoogleLoginRequest request)
    {
        // TODO: Replace stub with Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync()
        var email = ExtractEmailFromGoogleToken(request.IdToken);
        if (string.IsNullOrEmpty(email))
            throw new InvalidOperationException(AuthConstants.InvalidGoogleTokenMessage);

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new User
            {
                UserName = email,
                Email = email,
                Role = UserRole.Customer,
                IsEmailVerified = true,
                IsActive = true,
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException(AuthConstants.GoogleUserCreationFailedMessage);

            await _unitOfWork.Repository<CustomerProfile>().AddAsync(new CustomerProfile
            {
                UserId = user.Id,
                FullName = email.Split('@')[0],
                CreatedAt = DateTime.UtcNow
            });
            await _unitOfWork.SaveChangesAsync();
        }

        if (!user.IsActive)
            throw new InvalidOperationException(AuthConstants.AccountDeactivatedMessage);

        var (accessToken, refreshToken) = _jwtHelper.GenerateTokens(user);
        await SaveRefreshToken(user, refreshToken);

        return new AuthResponse
        {
            Id = user.Id,
            Token = accessToken,
            RefreshToken = refreshToken,
            Role = user.Role,
            Email = user.Email!,
            Name = await ResolveDisplayNameAsync(user),
            Message = AuthConstants.GoogleLoginSuccessMessage
        };
    }

    // ═══════════════════════════════════════════════════════════
    // Login
    // ═══════════════════════════════════════════════════════════
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new InvalidOperationException(AuthConstants.InvalidCredentialsMessage);

        if (!user.IsActive || user.IsDeleted)
            throw new InvalidOperationException(AuthConstants.AccountDeactivatedMessage);

        if (!user.IsEmailVerified)
            throw new InvalidOperationException(AuthConstants.EmailNotVerifiedMessage);

        if (user.Role == UserRole.Vendor)
        {
            var vendor = await _unitOfWork.Repository<VendorProfile>()
                .FirstOrDefaultAsync(v => v.UserId == user.Id);
            if (vendor != null && vendor.ApprovalStatus != ApprovalStatus.Approved)
                throw new InvalidOperationException(AuthConstants.VendorPendingApprovalMessage);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            throw new InvalidOperationException(AuthConstants.InvalidCredentialsMessage);

        if (user.Role == UserRole.Admin && user.IsMfaEnabled)
        {
            return new AuthResponse
            {
                Email = user.Email!,
                Role = user.Role,
                RequiresMfa = true,
                Message = AuthConstants.MfaRequiredMessage
            };
        }

        var (accessToken, refreshToken) = _jwtHelper.GenerateTokens(user);
        await SaveRefreshToken(user, refreshToken);

        return new AuthResponse
        {
            Id = user.Id,
            Token = accessToken,
            RefreshToken = refreshToken,
            Role = user.Role,
            Email = user.Email!,
            Name = await ResolveDisplayNameAsync(user),
            Message = AuthConstants.LoginSuccessMessage
        };
    }

    // ═══════════════════════════════════════════════════════════
    // Admin Login
    // ═══════════════════════════════════════════════════════════
    public async Task<AuthResponse> AdminLoginAsync(AdminLoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || user.Role != UserRole.Admin)
            throw new InvalidOperationException(AuthConstants.InvalidAdminCredentialsMessage);

        if (!user.IsActive || user.IsDeleted)
            throw new InvalidOperationException(AuthConstants.AccountDeactivatedMessage);

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            throw new InvalidOperationException(AuthConstants.InvalidAdminCredentialsMessage);

        if (user.IsMfaEnabled)
        {
            return new AuthResponse
            {
                Email = user.Email!,
                Role = user.Role,
                RequiresMfa = true,
                Message = AuthConstants.AdminMfaRequiredMessage
            };
        }

        var (accessToken, refreshToken) = _jwtHelper.GenerateTokens(user);
        await SaveRefreshToken(user, refreshToken);

        return new AuthResponse
        {
            Id = user.Id,
            Token = accessToken,
            RefreshToken = refreshToken,
            Role = user.Role,
            Email = user.Email!,
            Name = await ResolveDisplayNameAsync(user),
            Message = AuthConstants.AdminLoginSuccessMessage
        };
    }

    // ═══════════════════════════════════════════════════════════
    // Session Lifecycle
    // ═══════════════════════════════════════════════════════════
    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new InvalidOperationException(AuthConstants.InvalidRefreshTokenMessage);

        var (accessToken, refreshToken) = _jwtHelper.GenerateTokens(user);
        await SaveRefreshToken(user, refreshToken);

        return new AuthResponse
        {
            Id = user.Id,
            Token = accessToken,
            RefreshToken = refreshToken,
            Role = user.Role,
            Email = user.Email!,
            Name = await ResolveDisplayNameAsync(user),
            Message = AuthConstants.TokenRefreshedMessage
        };
    }

    public async Task LogoutAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // Password Reset
    // ═══════════════════════════════════════════════════════════
    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Always return success to avoid email enumeration
        if (user == null || user.Role == UserRole.Admin || user.IsDeleted)
            return;

        var code = GenerateSixDigitCode();
        user.PasswordResetCode = code;
        user.PasswordResetCodeExpiry = DateTime.UtcNow.AddMinutes(AuthConstants.PasswordResetOtpExpiryMinutes);
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Best-effort for the same reason as RegisterAsync — the code is
        // already persisted, and this endpoint always returns a generic
        // response regardless (to avoid email enumeration), so a delivery
        // failure here must not surface as a 500.
        try
        {
            await _emailService.SendPasswordResetOtpAsync(user.Email!, code);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send password reset OTP email to {Email}", user.Email);
        }
    }

    /// <summary>Step 1 of 2: verify the reset OTP code is valid.</summary>
    public async Task<bool> VerifyResetCodeAsync(VerifyResetCodeRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null) return false;
        if (user.PasswordResetCode != request.Code) return false;
        if (user.PasswordResetCodeExpiry < DateTime.UtcNow) return false;

        return true;
    }

    /// <summary>Step 2 of 2: apply new password using email + OTP code for identity.</summary>
    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null) return false;
        if (user.PasswordResetCode != request.Code) return false;
        if (user.PasswordResetCodeExpiry < DateTime.UtcNow) return false;

        // Generate a proper Identity reset token for the actual password change
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

        if (!result.Succeeded) return false;

        // Invalidate the OTP so it cannot be reused
        user.PasswordResetCode = null;
        user.PasswordResetCodeExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return true;
    }

    // ═══════════════════════════════════════════════════════════
    // Private Helpers
    // ═══════════════════════════════════════════════════════════
    private async Task SaveRefreshToken(User user, string refreshToken)
    {
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(AuthConstants.RefreshTokenExpiryDays);
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
    }

    private static string GenerateSixDigitCode() =>
        Random.Shared.Next(AuthConstants.OtpMinValue, AuthConstants.OtpMaxValue).ToString();

    /// <summary>Resolves the display name to return on AuthResponse — the
    /// frontend's AuthUser.name has no other source at login time.</summary>
    private async Task<string> ResolveDisplayNameAsync(User user)
    {
        if (user.Role == UserRole.Customer)
        {
            var profile = await _unitOfWork.Repository<CustomerProfile>()
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
            return profile?.FullName ?? string.Empty;
        }

        if (user.Role == UserRole.Vendor)
        {
            var profile = await _unitOfWork.Repository<VendorProfile>()
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
            return profile?.BusinessName ?? string.Empty;
        }

        // Admin has no profile table to source a name from.
        return string.Empty;
    }

    private static string ExtractEmailFromGoogleToken(string idToken)
    {
        // TODO: Replace with real validation using Google.Apis.Auth:
        // var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
        // return payload.Email;
        return string.Empty;
    }
}
