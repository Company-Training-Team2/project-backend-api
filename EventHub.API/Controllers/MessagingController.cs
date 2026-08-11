using System.Security.Claims;
using EventHub.Application.DTOs.Messaging;
using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

/// <summary>
/// Vendor&lt;-&gt;Customer direct messaging ("Contact Vendor"). Open to any
/// authenticated Customer or Vendor — a Conversation only ever has one of
/// each on it, enforced in MessagingService, not by a role gate here.
///
/// Routes:
///   GET  /api/messaging/conversations                → my threads (either side)
///   POST /api/messaging/conversations                → customer starts/resumes a thread with a listing's vendor
///   GET  /api/messaging/conversations/{id}/messages   → full thread; marks the other side's messages read
///   POST /api/messaging/conversations/{id}/messages   → reply
/// </summary>
[ApiController]
[Route("api/messaging")]
[Authorize]
public class MessagingController : ControllerBase
{
    private readonly IMessagingService _messagingService;

    public MessagingController(IMessagingService messagingService)
    {
        _messagingService = messagingService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var conversations = await _messagingService.GetMyConversationsAsync(GetCurrentUserId());
        return Ok(conversations);
    }

    [HttpPost("conversations")]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationDto dto)
    {
        try
        {
            var conversation = await _messagingService.CreateConversationAsync(GetCurrentUserId(), dto);
            return Ok(conversation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("conversations/{id:int}/messages")]
    public async Task<IActionResult> GetMessages(int id)
    {
        try
        {
            var messages = await _messagingService.GetMessagesAsync(id, GetCurrentUserId());
            return Ok(messages);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("conversations/{id:int}/messages")]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendConversationMessageDto dto)
    {
        try
        {
            var message = await _messagingService.SendMessageAsync(id, GetCurrentUserId(), dto);
            return Ok(message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
