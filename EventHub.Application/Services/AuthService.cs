using EventHub.Application.DTOs;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IMfaService _mfaService;
    private readonly JwtHelper _jwtHelper;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IMfaService mfaService,
        JwtHelper jwtHelper)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _mfaService = mfaService;
        _jwtHelper = jwtHelper;
    }

    // ═══════════════════════════════════════════════════════════
    // Register
    // ═══════════════════════════════════════════════════════════
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (request.Role == UserRole.Admin)
            throw new InvalidOperationException("Admin registration is not allowed.");

        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            throw new InvalidOperationException("Email already registered.");

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

        // ── OTP email verification (audit Module 1) ────────────────────────────
        var otpCode = GenerateSixDigitCode();
        user.EmailVerificationCode = otpCode;
        user.EmailVerificationExpiry = DateTime.UtcNow.AddMinutes(15);
        await _userManager.UpdateAsync(user);

        await _emailService.SendVerificationOtpAsync(user.Email, otpCode);

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
            Message = "Registration successful. Please check your email for the 6-digit verification code."
        };
    }

    // ═══════════════════════════════════════════════════════════
    // Email OTP Verification (audit Module 1)
    // ═══════════════════════════════════════════════════════════
    public async Task<bool> VerifyEmailOtpAsync(VerifyEmailOtpRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return false;

        if (user.EmailVerificationCode != request.Code)
            return false;

        if (user.EmailVerificationExpiry < DateTime.UtcNow)
            return false;

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
            throw new InvalidOperationException("Invalid Google token.");

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
                throw new InvalidOperationException("Failed to create user from Google login.");

            await _unitOfWork.Repository<CustomerProfile>().AddAsync(new CustomerProfile
            {
                UserId = user.Id,
                FullName = email.Split('@')[0],
                CreatedAt = DateTime.UtcNow
            });
            await _unitOfWork.SaveChangesAsync();
        }

        if (!user.IsActive)
            throw new InvalidOperationException("Account is deactivated.");

        var (accessToken, refreshToken) = _jwtHelper.GenerateTokens(user);
        await SaveRefreshToken(user, refreshToken);

        return new AuthResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            Role = user.Role,
            Email = user.Email!,
            Message = "Google login successful."
        };
    }

    // ═══════════════════════════════════════════════════════════
    // Login
    // ═══════════════════════════════════════════════════════════
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new InvalidOperationException("Invalid email or password.");

        if (!user.IsActive || user.IsDeleted)
            throw new InvalidOperationException("Account is deactivated.");

        if (!user.IsEmailVerified)
            throw new InvalidOperationException("Email not verified. Please enter the 6-digit code sent to your inbox.");

        if (user.Role == UserRole.Vendor)
        {
            var vendor = await _unitOfWork.Repository<VendorProfile>()
                .FirstOrDefaultAsync(v => v.UserId == user.Id);
            if (vendor != null && vendor.ApprovalStatus != ApprovalStatus.Approved)
                throw new InvalidOperationException("Vendor account is pending admin approval.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            throw new InvalidOperationException("Invalid email or password.");

        if (user.Role == UserRole.Admin && user.IsMfaEnabled)
        {
            return new AuthResponse
            {
                Email = user.Email!,
                Role = user.Role,
                RequiresMfa = true,
                Message = "MFA required."
            };
        }

        var (accessToken, refreshToken) = _jwtHelper.GenerateTokens(user);
        await SaveRefreshToken(user, refreshToken);

        return new AuthResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            Role = user.Role,
            Email = user.Email!,
            Message = "Login successful."
        };
    }

    // ═══════════════════════════════════════════════════════════
    // Admin Login
    // ═══════════════════════════════════════════════════════════
    public async Task<AuthResponse> AdminLoginAsync(AdminLoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || user.Role != UserRole.Admin)
            throw new InvalidOperationException("Invalid admin credentials.");

        if (!user.IsActive || user.IsDeleted)
            throw new InvalidOperationException("Account is deactivated.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            throw new InvalidOperationException("Invalid admin credentials.");

        if (user.IsMfaEnabled)
        {
            return new AuthResponse
            {
                Email = user.Email!,
                Role = user.Role,
                RequiresMfa = true,
                Message = "MFA required. Please enter your authenticator code."
            };
        }

        var (accessToken, refreshToken) = _jwtHelper.GenerateTokens(user);
        await SaveRefreshToken(user, refreshToken);

        return new AuthResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            Role = user.Role,
            Email = user.Email!,
            Message = "Admin login successful."
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
            throw new InvalidOperationException("Invalid or expired refresh token.");

        var (accessToken, refreshToken) = _jwtHelper.GenerateTokens(user);
        await SaveRefreshToken(user, refreshToken);

        return new AuthResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            Role = user.Role,
            Email = user.Email!,
            Message = "Token refreshed."
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
    // Password Reset (audit Module 1 — fixed)
    // ═══════════════════════════════════════════════════════════
    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Always return success to avoid email enumeration
        if (user == null || user.Role == UserRole.Admin || user.IsDeleted)
            return;

        var code = GenerateSixDigitCode();
        user.PasswordResetCode = code;
        user.PasswordResetCodeExpiry = DateTime.UtcNow.AddMinutes(15);
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        await _emailService.SendPasswordResetOtpAsync(user.Email!, code);
    }

    /// <summary>Step 1 of 2: verify the reset OTP code is valid.</summary>
    public async Task<bool> VerifyResetCodeAsync(VerifyResetCodeRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return false;

        if (user.PasswordResetCode != request.Code)
            return false;

        if (user.PasswordResetCodeExpiry < DateTime.UtcNow)
            return false;

        return true;
    }

    /// <summary>Step 2 of 2: apply new password using email + OTP code for identity.</summary>
    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return false;

        if (user.PasswordResetCode != request.Code)
            return false;

        if (user.PasswordResetCodeExpiry < DateTime.UtcNow)
            return false;

        // Generate a proper Identity reset token for the actual password change
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

        if (!result.Succeeded)
            return false;

        // Invalidate the OTP so it cannot be reused
        user.PasswordResetCode = null;
        user.PasswordResetCodeExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return true;
    }

    // ═══════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════
    private async Task SaveRefreshToken(User user, string refreshToken)
    {
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
    }

    private static string GenerateSixDigitCode() =>
        Random.Shared.Next(100_000, 999_999).ToString();

    private static string ExtractEmailFromGoogleToken(string idToken)
    {
        // TODO: Replace with real validation using Google.Apis.Auth:
        // var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
        // return payload.Email;
        return string.Empty;
    }
}