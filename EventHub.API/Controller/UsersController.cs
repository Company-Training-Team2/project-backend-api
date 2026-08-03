using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventHub.Application.DTOs;
using EventHub.Application.Interfaces;
namespace EventHub.API.Controllers;

[ApiController]
[Route("api/users")]
//[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // GET: api/users/me
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var user = await _userService.GetCurrentUserAsync();

        return Ok(user);
    }


    // PUT: api/users/me
    // PUT: api/users/me
    // PUT: api/users/me
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(UpdateUserDto dto)
    {
        var user = await _userService.UpdateUserAsync(dto);

        return Ok(user);
    }
    [HttpDelete("me/deactivate")]
public async Task<IActionResult> DeactivateAccount()
{
    await _userService.DeactivateAccountAsync();

        return NoContent();
    }
    // POST: api/users/confirm-email
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(
        ConfirmEmailDto dto)
    {
        var result = await _userService.ConfirmEmailAsync(dto.Token);

        return Ok(new
        {
            message = "Email confirmed successfully"
        });
    }
}