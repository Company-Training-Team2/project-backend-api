using System.IdentityModel.Tokens.Jwt;
using EventHub.Application.DTOs.Auth;
using EventHub.Application.Helpers;
using EventHub.Application.Interfaces;
using EventHub.Domain.Constants;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

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
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;

    /// <summary>
    /// REG-CUS-013: How long a completed registration idempotency key is retained.
    /// Any duplicate request carrying the same key within this window receives the
    /// cached success response rather than attempting a second account creation.
    /// </summary>
    private static readonly TimeSpan IdempotencyKeyTtl = TimeSpan.FromMinutes(5);

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IMfaService mfaService,
        JwtHelper jwtHelper,
        ILogger<AuthService> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _mfaService = mfaService;
        _jwtHelper = jwtHelper;
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
    }

    // ═══════════════════════════════════════════════════════════
    // Register
    // ═══════════════════════════════════════════════════════════
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (request.Role == UserRole.Admin)
            throw new InvalidOperationException(AuthConstants.AdminRegistrationForbiddenMessage);

        // REG-CUS-013: Idempotency guard — if the client supplied a key and we
        // already completed a registration with that exact key, return the cached
        // success response without touching the database again.  This makes rapid
        // multi-click (or network-retry) safe: only the first request creates the
        // account; subsequent duplicates within IdempotencyKeyTtl get the same 200.
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var cacheKey = $"reg:idem:{request.IdempotencyKey}";
            if (_cache.TryGetValue(cacheKey, out AuthResponse? cached) && cached is not null)
            {
                _logger.LogInformation(
                    "Duplicate registration request suppressed via idempotency key {Key}",
                    request.IdempotencyKey);
                return cached;
            }
        }

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
            var vendorProfile = new VendorProfile
            {
                UserId = user.Id,
                BusinessName = request.BusinessName ?? string.Empty,
                BioDescription = request.BioDescription ?? string.Empty,
                ApprovalStatus = ApprovalStatus.Pending,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            // Up to 3 service categories chosen on Step 2 of vendor registration.
            // Unknown/invalid ids are silently dropped rather than failing the
            // whole registration over a bad category id.
            if (request.CategoryIds is { Count: > 0 })
            {
                var requestedIds = request.CategoryIds.Distinct().Take(3).ToList();
                var validCategories = await _unitOfWork.Repository<Category>()
                    .FindAsync(c => requestedIds.Contains(c.Id));

                foreach (var category in validCategories)
                {
                    vendorProfile.VendorCategories.Add(new VendorProfileCategory { Category = category });
                }
            }

            await _unitOfWork.Repository<VendorProfile>().AddAsync(vendorProfile);
        }

        await _unitOfWork.SaveChangesAsync();

        var response = new AuthResponse
        {
            Message = AuthConstants.RegistrationSuccessMessage
        };

        // REG-CUS-013: Store the completed response so any duplicate request
        // carrying the same idempotency key within the TTL window is short-circuited
        // above and returns this same success rather than hitting the DB again.
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            _cache.Set(
                $"reg:idem:{request.IdempotencyKey}",
                response,
                IdempotencyKeyTtl);
        }

        return response;
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
    // Resend Email OTP
    // ═══════════════════════════════════════════════════════════
    public async Task ResendEmailOtpAsync(ResendOtpRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new InvalidOperationException(AuthConstants.AccountNotFoundMessage);

        if (user.IsEmailVerified)
            throw new InvalidOperationException(AuthConstants.EmailAlreadyVerifiedMessage);

        // Reuse the existing expiry to derive when the last code was actually
        // sent (expiry = sentAt + EmailOtpExpiryMinutes), so we can enforce a
        // cooldown without needing a separate "last sent" column.
        if (user.EmailVerificationExpiry.HasValue)
        {
            var lastSentAt = user.EmailVerificationExpiry.Value.AddMinutes(-AuthConstants.EmailOtpExpiryMinutes);
            var secondsSinceLastSend = (DateTime.UtcNow - lastSentAt).TotalSeconds;

            if (secondsSinceLastSend < AuthConstants.ResendOtpCooldownSeconds)
            {
                var secondsRemaining = (int)Math.Ceiling(AuthConstants.ResendOtpCooldownSeconds - secondsSinceLastSend);
                throw new InvalidOperationException(AuthConstants.ResendOtpCooldownMessage(secondsRemaining));
            }
        }

        var otpCode = GenerateSixDigitCode();
        user.EmailVerificationCode = otpCode;
        user.EmailVerificationExpiry = DateTime.UtcNow.AddMinutes(AuthConstants.EmailOtpExpiryMinutes);
        await _userManager.UpdateAsync(user);

        try
        {
            await _emailService.SendVerificationOtpAsync(user.Email!, otpCode);
        }
        catch (Exception)
        {
            throw new InvalidOperationException(AuthConstants.OtpSendFailedMessage);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // Google Login
    // ═══════════════════════════════════════════════════════════
    public async Task<AuthResponse> GoogleAuthAsync(GoogleLoginRequest request)
    {
        var email = await ValidateGoogleIdTokenAsync(request.IdToken);
        if (string.IsNullOrEmpty(email))
            throw new InvalidOperationException(AuthConstants.InvalidGoogleTokenMessage);

        return await LoginOrCreateSocialUserAsync(
            email,
            AuthConstants.GoogleUserCreationFailedMessage,
            AuthConstants.GoogleLoginSuccessMessage);
    }

    // ═══════════════════════════════════════════════════════════
    // Apple Login ("Sign in with Apple" — client-side popup flow,
    // frontend hands us the id_token Apple issued directly; no private
    // key / client_secret needed on our side for this, only for the
    // server-side authorization-code flow, which this isn't using)
    // ═══════════════════════════════════════════════════════════
    public async Task<AuthResponse> AppleAuthAsync(AppleLoginRequest request)
    {
        var email = await ValidateAppleIdTokenAsync(request.IdToken);
        if (string.IsNullOrEmpty(email))
            throw new InvalidOperationException(AuthConstants.InvalidAppleTokenMessage);

        return await LoginOrCreateSocialUserAsync(
            email,
            AuthConstants.AppleUserCreationFailedMessage,
            AuthConstants.AppleLoginSuccessMessage);
    }

    /// <summary>Shared by GoogleAuthAsync/AppleAuthAsync — both providers hand us a
    /// verified email and nothing else distinguishes how we log the user in: find-or-create
    /// a Customer account, then issue our own tokens same as any other login.</summary>
    private async Task<AuthResponse> LoginOrCreateSocialUserAsync(
        string email, string creationFailedMessage, string successMessage)
    {
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
                throw new InvalidOperationException(creationFailedMessage);

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
            Message = successMessage
        };
    }

    /// <summary>Verifies a Google Sign-In id_token: fetches Google's current signing
    /// keys, checks the signature against the key matching the token's "kid", and
    /// checks issuer/audience/expiry. Returns null (never throws) on any failure —
    /// including "not configured yet", so callers just see "invalid token" either way.</summary>
    private Task<string?> ValidateGoogleIdTokenAsync(string idToken)
    {
        var clientId = _configuration["GoogleAuth:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogWarning("Google sign-in attempted but GoogleAuth:ClientId is not configured.");
            return Task.FromResult<string?>(null);
        }

        return ValidateOidcIdTokenAsync(
            idToken,
            jwksUrl: "https://www.googleapis.com/oauth2/v3/certs",
            validIssuers: ["https://accounts.google.com", "accounts.google.com"],
            audience: clientId);
    }

    /// <summary>Same idea as ValidateGoogleIdTokenAsync, verified against Apple's
    /// published signing keys instead. This is the id_token Apple's JS SDK
    /// (AppleID.auth.signIn(), popup mode) hands back to the browser directly — no
    /// server-side authorization-code exchange, so no private key/client_secret is
    /// needed here.</summary>
    private Task<string?> ValidateAppleIdTokenAsync(string idToken)
    {
        var clientId = _configuration["AppleAuth:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogWarning("Apple sign-in attempted but AppleAuth:ClientId is not configured.");
            return Task.FromResult<string?>(null);
        }

        return ValidateOidcIdTokenAsync(
            idToken,
            jwksUrl: "https://appleid.apple.com/auth/keys",
            validIssuers: ["https://appleid.apple.com"],
            audience: clientId);
    }

    /// <summary>Generic OIDC id_token verification shared by Google and Apple — both
    /// are standard signed JWTs published against a JWKS endpoint, so one
    /// implementation covers both providers; only the JWKS URL/issuer/audience differ.
    /// Never throws: any failure (bad signature, wrong audience, expired, network
    /// error reaching the JWKS endpoint, etc.) just returns null, and callers turn
    /// that into a generic "invalid token" error — deliberately not distinguishing
    /// *why* verification failed in the response, to avoid leaking details useful for
    /// forging a token.</summary>
    private async Task<string?> ValidateOidcIdTokenAsync(
        string idToken, string jwksUrl, string[] validIssuers, string audience)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var kid = handler.ReadJwtToken(idToken).Header.Kid;
            if (string.IsNullOrEmpty(kid))
                return null;

            var httpClient = _httpClientFactory.CreateClient();
            var jwksJson = await httpClient.GetStringAsync(jwksUrl);
            var signingKey = new JsonWebKeySet(jwksJson).Keys
                .FirstOrDefault(k => k.Kid == kid);
            if (signingKey == null)
                return null;

            var principal = handler.ValidateToken(idToken, new TokenValidationParameters
            {
                ValidIssuers = validIssuers,
                ValidAudience = audience,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
            }, out _);

            var email = principal.FindFirst("email")?.Value;
            return string.IsNullOrWhiteSpace(email) ? null : email;
        }
        catch
        {
            return null;
        }
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

        // REG-CUS-022: Surface email-not-verified as a structured flag rather than
        // a bare exception so the frontend can navigate to the Email Verification
        // OTP screen directly.  Using a dedicated flag keeps this flow strictly
        // separate from the Forgot Password flow (which uses its own OTP screen),
        // preventing the navigation bleed observed in the test.
        if (!user.IsEmailVerified)
        {
            return new AuthResponse
            {
                Email = user.Email!,
                Role = user.Role,
                RequiresEmailVerification = true,
                Message = AuthConstants.EmailNotVerifiedMessage
            };
        }

        if (user.Role == UserRole.Vendor)
        {
            var vendor = await _unitOfWork.Repository<VendorProfile>()
                .FirstOrDefaultAsync(v => v.UserId == user.Id);
            if (vendor != null && vendor.ApprovalStatus != ApprovalStatus.Approved)
                throw new InvalidOperationException(AuthConstants.VendorPendingApprovalMessage);
        }

        // lockoutOnFailure: true — 5 failed attempts (Lockout.MaxFailedAccessAttempts,
        // configured in Program.cs) locks the account for 5 minutes via
        // Identity's own AccessFailedCount/LockoutEnd tracking on AspNetUsers.
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
            throw new InvalidOperationException(AuthConstants.AccountLockedOutMessage);
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

        // Always return success to avoid email enumeration.
        // REG-CUS-022: Also silently no-op for accounts that have not yet verified
        // their email — these are still inside the Email Verification flow and must
        // not be able to enter the Forgot Password flow.  This prevents the
        // password-reset OTP screen from becoming reachable from the email-
        // verification OTP screen, which was the root cause of the navigation bleed.
        if (user == null || user.Role == UserRole.Admin || user.IsDeleted || !user.IsEmailVerified)
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
}
