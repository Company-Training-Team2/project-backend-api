using System.Security.Claims;
using EventHub.API.DTOs.AI;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Infrastructure.Persistence.Context;
using EventHub.Infrastructure.Services.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventHub.Domain.Enums;

namespace EventHub.API.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/ai-planner")]
// Was previously reachable by anyone, authenticated or not — any caller could
// read/write any other customer's AI conversation just by guessing an
// eventId. [Authorize] + the ownership check below (mirrors EventsController's
// GetCurrentUserId()/IEventService.EventBelongsToUserAsync() pattern) close
// that gap.
[Authorize]
public class AIPlannerController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAIService _aiService;
    private readonly IEventService _eventService;

    public AIPlannerController(
        ApplicationDbContext context,
        IAIService aiService,
        IEventService eventService)
    {
        _context = context;
        _aiService = aiService;
        _eventService = eventService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.Parse(userIdClaim!);
    }

    [HttpPost("ask")]
    public async Task<IActionResult> AskAI(
        int eventId,
        AskAIRequest request)
    {
        var userId = GetCurrentUserId();
        if (!await _eventService.EventBelongsToUserAsync(eventId, userId))
            return NotFound("Event not found.");

        var eventEntity = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventEntity == null)
            return NotFound("Event not found.");

        var conversation = await _context.AIConversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.EventId == eventId);

        if (conversation == null)
        {
            conversation = new AIConversation
            {
                EventId = eventId,
                Title = "AI Conversation"
            };

            _context.AIConversations.Add(conversation);
            await _context.SaveChangesAsync();
        }

        var userMessage = new AIMessage
        {
            AIConversationId = conversation.Id,
            Sender = AIMessageSender.User,
            Content = request.Prompt,
            CreatedAt = DateTime.UtcNow
        };

        _context.AIMessages.Add(userMessage);

        var aiResponse = await _aiService.GenerateResponseAsync(
            eventEntity,
            conversation.Messages.ToList(),
            request.Prompt);

        var aiMessage = new AIMessage
        {
            AIConversationId = conversation.Id,
            Sender = AIMessageSender.Assistant,
            Content = aiResponse,
            CreatedAt = DateTime.UtcNow
        };

        _context.AIMessages.Add(aiMessage);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Response = aiResponse
        });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(int eventId)
    {
        var userId = GetCurrentUserId();
        if (!await _eventService.EventBelongsToUserAsync(eventId, userId))
            return NotFound("Event not found.");

        var conversation = await _context.AIConversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.EventId == eventId);

        if (conversation == null)
            return Ok(new List<AIMessageDto>());

        var result = conversation.Messages
            .OrderBy(x => x.CreatedAt)
            .Select(x => new AIMessageDto
            {
                Sender = x.Sender,
                Content = x.Content,
                CreatedAt = x.CreatedAt
            });

        return Ok(result);
    }
}
