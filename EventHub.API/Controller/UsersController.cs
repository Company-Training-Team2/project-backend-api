using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventHub.Application.DTOs;
using EventHub.Application.Interfaces;
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

    // GET: api/users/me
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var user = await _userService.GetCurrentUserAsync();

        return Ok(user);
    }


    // PUT: api/users/me
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(UpdateUserDto dto)
    {
        var result = await _userService.UpdateUserAsync(dto);

        return Ok(result);
    }
    [HttpDelete("me")]
public async Task<IActionResult> DeactivateAccount()
{
    await _userService.DeactivateAccountAsync();

    return Ok(new
    {
        message = "Account deactivated successfully"
    });
}
}