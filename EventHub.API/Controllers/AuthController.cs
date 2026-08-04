using EventHub.Application.DTOs;
using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IMfaService _mfaService;

    public AuthController(IAuthService authService, IMfaService mfaService)
    {
        _authService = authService;
        _mfaService = mfaService;
    }

    // ═══════════════════════════════════════════════════════════
    // TASK 2: Register
    // ═══════════════════════════════════════════════════════════
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

    // ═══════════════════════════════════════════════════════════
    // TASK 3: Google Login
    // ═══════════════════════════════════════════════════════════
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

    // ═══════════════════════════════════════════════════════════
    // TASK 4: Email Verification
    // ═══════════════════════════════════════════════════════════
    [HttpGet("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        var success = await _authService.VerifyEmailAsync(token);
        if (success)
            return Ok(new { message = "Email verified successfully." });
        return BadRequest(new { message = "Invalid or expired token." });
    }

    // ═══════════════════════════════════════════════════════════
    // TASK 5: Login
    // ═══════════════════════════════════════════════════════════
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

    // ═══════════════════════════════════════════════════════════
    // TASK 6: Admin Login (isolated route)
    // ═══════════════════════════════════════════════════════════
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

    // ═══════════════════════════════════════════════════════════
    // TASK 7: MFA
    // ═══════════════════════════════════════════════════════════
    [HttpPost("admin/mfa/setup")]
    [Authorize(Roles = "Admin")]
    public IActionResult SetupMfa()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        var setup = _mfaService.GenerateSetup(email);
        return Ok(setup);
    }

    [HttpPost("admin/mfa/verify")]
    [AllowAnonymous]
    public IActionResult VerifyMfa([FromBody] MfaVerifyRequest request)
    {
        // TODO: Complete MFA verification flow
        // This is a simplified version - in production, verify against stored secret

        return Ok(new { message = "MFA verification endpoint." });
    }

    // ═══════════════════════════════════════════════════════════
    // TASK 8: Session Lifecycle
    // ═══════════════════════════════════════════════════════════
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

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        await _authService.LogoutAsync(userId);
        return Ok(new { message = "Logged out successfully." });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _authService.ForgotPasswordAsync(request);
        return Ok(new { message = "If the email exists, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var success = await _authService.ResetPasswordAsync(request);
        if (success)
            return Ok(new { message = "Password reset successfully." });
        return BadRequest(new { message = "Invalid or expired token." });
    }
}
