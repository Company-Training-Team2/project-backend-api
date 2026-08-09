using EventHub.Application.DTOs.Auth;
using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventHub.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IMfaService _mfaService;

    public AuthController(IAuthService authService, IMfaService mfaService)
    {
        _authService = authService;
        _mfaService = mfaService;
    }

    // ── Register ──────────────────────────────────────────────────────────────
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── OTP Email Verification (audit Module 1) ───────────────────────────────
    /// <summary>POST /api/auth/verify-email — accepts { email, code } 6-digit OTP.</summary>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailOtpRequest request)
    {
        var success = await _authService.VerifyEmailOtpAsync(request);

        return success
            ? Ok(new { message = "Email verified successfully." })
            : BadRequest(new { message = "Invalid or expired verification code." });
    }

    // ── Google Login ──────────────────────────────────────────────────────────
    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        try
        {
            var response = await _authService.GoogleAuthAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── Apple Login ───────────────────────────────────────────────────────────
    [HttpPost("apple")]
    [AllowAnonymous]
    public async Task<IActionResult> AppleLogin([FromBody] AppleLoginRequest request)
    {
        try
        {
            var response = await _authService.AppleAuthAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── Login ─────────────────────────────────────────────────────────────────
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            if (response.RequiresMfa)
                return Ok(new { requiresMfa = true, email = response.Email, message = response.Message });
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── Admin Login ───────────────────────────────────────────────────────────
    [HttpPost("admin/login")]
    [AllowAnonymous]
    public async Task<IActionResult> AdminLogin([FromBody] AdminLoginRequest request)
    {
        try
        {
            var response = await _authService.AdminLoginAsync(request);
            if (response.RequiresMfa)
                return Ok(new { requiresMfa = true, email = response.Email, message = response.Message });
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── MFA Setup (Admin only) ────────────────────────────────────────────────
    [HttpPost("admin/mfa/setup")]
    [Authorize(Roles = "Admin")]
    public IActionResult SetupMfa()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        var setup = _mfaService.GenerateSetup(email);
        return Ok(setup);
    }

    // ── MFA Verify (Admin only) ───────────────────────────────────────────────
    [HttpPost("admin/mfa/verify")]
    [AllowAnonymous]
    public IActionResult VerifyMfa([FromBody] MfaVerifyRequest request)
    {
        // TODO: Load MfaSecret from user record, call _mfaService.ValidateCode(),
        //       then issue JWT on success. Currently a stub per existing codebase.
        return Ok(new { message = "MFA verification not yet implemented." });
    }

    // ── Token Refresh ─────────────────────────────────────────────────────────
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── Logout ────────────────────────────────────────────────────────────────
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _authService.LogoutAsync(userId);
        return Ok(new { message = "Logged out successfully." });
    }

    // ── Forgot Password ───────────────────────────────────────────────────────
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _authService.ForgotPasswordAsync(request);
        return Ok(new { message = "If the email exists, a 6-digit reset code has been sent." });
    }

    // ── Verify Reset Code (audit Module 1: new endpoint) ─────────────────────
    [HttpPost("verify-reset-code")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeRequest request)
    {
        var valid = await _authService.VerifyResetCodeAsync(request);
        return valid
            ? Ok(new { message = "Code verified. You may now submit your new password." })
            : BadRequest(new { message = "Invalid or expired reset code." });
    }

    // ── Reset Password ────────────────────────────────────────────────────────
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var success = await _authService.ResetPasswordAsync(request);
        return success
            ? Ok(new { message = "Password reset successfully." })
            : BadRequest(new { message = "Invalid or expired reset code." });
    }
}