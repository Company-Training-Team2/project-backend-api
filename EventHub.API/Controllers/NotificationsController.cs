using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

/// <summary>
/// Audit Module 10 (Notifications): event-driven inbox system (booking status
/// updates, vendor matches, system security alerts, payment receipts, reviews,
/// messages) grouped chronologically (Today, Yesterday, Earlier).
/// Requires authentication — the feed is scoped to the logged-in user.
/// </summary>
[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>GET /api/notifications — retrieve the structured notification feed.</summary>
    [HttpGet]
    public async Task<IActionResult> GetFeed()
    {
        var result = await _notificationService.GetFeedAsync();

        return Ok(result);
    }

    /// <summary>PATCH /api/notifications/{id}/read — mark a specific notification as read.</summary>
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var result = await _notificationService.MarkAsReadAsync(id);

        return Ok(result);
    }
}
