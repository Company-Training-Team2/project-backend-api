using EventHub.Application.DTOs;
using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // ── GET /api/users/me ─────────────────────────────────────────────────────
    // Audit Module 12: refactored — returns real user data, not mock.
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var user = await _userService.GetCurrentUserAsync();
            return Ok(user);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    // ── PUT /api/users/me ─────────────────────────────────────────────────────
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto dto)
    {
        try
        {
            var user = await _userService.UpdateProfileAsync(dto);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── PUT /api/users/me/password ────────────────────────────────────────────
    // Audit Module 12: dedicated password change — validate DTO & identity mapping.
    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            await _userService.ChangePasswordAsync(dto);
            return Ok(new { message = "Password changed successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── DELETE /api/users/me/deactivate ───────────────────────────────────────
    [HttpDelete("me/deactivate")]
    public async Task<IActionResult> DeactivateAccount()
    {
        try
        {
            await _userService.DeactivateAccountAsync();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── GET /api/users/me/activity ────────────────────────────────────────────
    // Audit Module 12: previously missing endpoint — now wired.
    [HttpGet("me/activity")]
    public async Task<IActionResult> GetActivityLog(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var activity = await _userService.GetActivityLogAsync(page, pageSize);
        return Ok(activity);
    }
}