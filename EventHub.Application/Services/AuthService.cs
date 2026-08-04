using EventHub.Application.DTOs;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EventHub.Application.Services; 

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
    // TASK 2: Register Email/Password
    // ═══════════════════════════════════════════════════════════
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Reject Admin registration
        if (request.Role == UserRole.Admin)
            throw new InvalidOperationException("Admin registration is not allowed.");

        // Check email uniqueness
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            throw new InvalidOperationException("Email already registered.");

        // Create User
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            Role = request.Role,
            IsEmailVerified = false,
            IsActive = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        // Generate verification token
        var verificationToken = Guid.NewGuid().ToString("N");
        user.EmailVerificationToken = verificationToken;
        user.EmailVerificationExpiry = DateTime.UtcNow.AddHours(24);
        await _userManager.UpdateAsync(user);

        // Send verification email
        await _emailService.SendVerificationEmailAsync(user.Email, verificationToken);

        // Create profile based on role
        if (request.Role == UserRole.Customer)
        {
            var profile = new CustomerProfile
            {
                UserId = user.Id,
                FullName = request.FullName ?? "",
                City = request.City
            };
            await _unitOfWork.Repository<CustomerProfile>().AddAsync(profile);
        }
        else if (request.Role == UserRole.Vendor)
        {
            var profile = new VendorProfile
            {
                UserId = user.Id,
                BusinessName = request.BusinessName ?? "",
                BioDescription = request.BioDescription ?? "",
                ApprovalStatus = ApprovalStatus.Pending,
                IsVerified = false
            };
            await _unitOfWork.Repository<VendorProfile>().AddAsync(profile);
        }

        await _unitOfWork.SaveChangesAsync();

        return new AuthResponse
        {
            Message = "Registration successful. Please verify your email."
        };
    }

    // ═══════════════════════════════════════════════════════════
    // TASK 3: Social Login (Google)
    // ═══════════════════════════════════════════════════════════
    public async Task<AuthResponse> GoogleAuthAsync(GoogleLoginRequest request)
    {
        // In real implementation, validate Google ID token here
        // For now, simulate by extracting email from token
        // Use Google.Apis.Auth package: GoogleJsonWebSignature.ValidateAsync()

        // Placeholder: extract email from token (replace with real validation)
        var email = ExtractEmailFromGoogleToken(request.IdToken);
        if (string.IsNullOrEmpty(email))
            throw new InvalidOperationException("Invalid Google token.");

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            // First time login → create user
            user = new User
            {
                UserName = email,
                Email = email,
                Role = UserRole.Customer, // Default for social login
                IsEmailVerified = true,   // Google already verified
                IsActive = true,
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException("Failed to create user from Google login.");

            // Create CustomerProfile
            var profile = new CustomerProfile
            {
                UserId = user.Id,
                FullName = email.Split('@')[0]
            };
            await _unitOfWork.Repository<CustomerProfile>().AddAsync(profile);
            await _unitOfWork.SaveChangesAsync();
        }

        // Check if active
        if (!user.IsActive)
            throw new InvalidOperationException("Account is deactivated.");

        // Generate tokens
        var (accessToken, refreshToken) = _jwtHelper.GenerateTokens(user);
        await SaveRefreshToken(user, refreshToken);

        return new AuthResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            Role = user.Role,
            Email = user.Email,
            Message = "Google login successful."
        };
    }

    // ═══════════════════════════════════════════════════════════
    // TASK 4: Email Verification
    // ═══════════════════════════════════════════════════════════
    public async Task<bool> VerifyEmailAsync(string token)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

        if (user == null)
            return false;

        if (user.EmailVerificationExpiry < DateTime.UtcNow)
            return false;

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationExpiry = null;
        user.EmailConfirmed = true;

        await _userManager.UpdateAsync(user);
        return true;
    }

    // ═══════════════════════════════════════════════════════════
    // TASK 5: Login + JWT
    // ═══════════════════════════════════════════════════════════
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new InvalidOperationException("Invalid email or password.");

        if (!user.IsActive)
            throw new InvalidOperationException("Account is deactivated.");

        if (!user.IsEmailVerified)
            throw new InvalidOperationException("Email not verified. Please check your inbox.");

        // Check vendor approval
        if (user.Role == UserRole.Vendor)
        {
            var vendor = await _unitOfWork.Repository<VendorProfile>()
                .FirstOrDefaultAsync(v => v.UserId == user.Id);
            if (vendor != null && vendor.ApprovalStatus != ApprovalStatus.Approved)
                throw new InvalidOperationException("Vendor account pending approval.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            throw new InvalidOperationException("Invalid email or password.");

        // Check MFA for Admin
        if (user.Role == UserRole.Admin && user.IsMfaEnabled)
        {
            return new AuthResponse
            {
                Email = user.Email,
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
            Email = user.Email,
            Message = "Login successful."
        };
    }

    // ═══════════════════════════════════════════════════════════
    // TASK 6: Admin Login (isolated)
    // ═══════════════════════════════════════════════════════════
    public async Task<AuthResponse> AdminLoginAsync(AdminLoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || user.Role != UserRole.Admin)
            throw new InvalidOperationException("Invalid admin credentials.");

        if (!user.IsActive)
            throw new InvalidOperationException("Account is deactivated.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            throw new InvalidOperationException("Invalid admin credentials.");

        // Check MFA
        if (user.IsMfaEnabled)
        {
            return new AuthResponse
            {
                Email = user.Email,
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
            Email = user.Email,
            Message = "Admin login successful."
        };
    }

    // ═══════════════════════════════════════════════════════════
    // TASK 8: Session Lifecycle (Refresh, Logout, Forgot, Reset)
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
            Email = user.Email,
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
            await _userManager.UpdateAsync(user);
        }
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || user.Role == UserRole.Admin)
            return false; // Don't reveal if email exists; Admin resets handled out-of-band

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _emailService.SendPasswordResetEmailAsync(user.Email, token);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        // Find user by reset token (Identity handles this internally)
        // We need email to reset - in real flow, token is tied to email
        // This is a simplified version
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == request.Token); // Reusing field temporarily

        if (user == null)
            return false;

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        return result.Succeeded;
    }

    // ═══════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════
    private async Task SaveRefreshToken(User user, string refreshToken)
    {
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);
    }

    private static string ExtractEmailFromGoogleToken(string idToken)
    {
        // TODO: Replace with real Google token validation
        // Using Google.Apis.Auth: GoogleJsonWebSignature.ValidateAsync(idToken)
        // For now, return a placeholder - this is NOT production ready
        return ""; 
    }
}
