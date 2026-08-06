using EventHub.Application.DTOs.Checklist;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;

namespace EventHub.Application.Services;

/// <summary>
/// Module 5 – Checklist task management.
///
/// Authorization pattern: identical to GuestService —
/// verify event ownership via IEventService.EventBelongsToUserAsync, then
/// operate on ChecklistItem via the generic IUnitOfWork repository.
/// </summary>
public class ChecklistService : IChecklistService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventService _eventService;

    public ChecklistService(IUnitOfWork unitOfWork, IEventService eventService)
    {
        _unitOfWork = unitOfWork;
        _eventService = eventService;
    }

    // ─── GET checklist ────────────────────────────────────────────────────────

    public async Task<EventChecklistResponse?> GetChecklistAsync(
        int eventId,
        int userId)
    {
        var owned = await _eventService.EventBelongsToUserAsync(eventId, userId);
        if (!owned)
            return null;

        var items = await _unitOfWork
            .Repository<ChecklistItem>()
            .FindAsync(c => c.EventId == eventId);

        var dtos = items.Select(MapToDto).ToList();

        var pending = dtos.Where(d => !d.IsCompleted)
                            .OrderByDescending(d => ParsePriority(d.Priority))
                            .ThenBy(d => d.DueDate)
                            .ToList();

        var completed = dtos.Where(d => d.IsCompleted)
                            .OrderByDescending(d => d.UpdatedAt)
                            .ToList();

        return new EventChecklistResponse
        {
            EventId = eventId,
            TotalCount = dtos.Count,
            CompletedCount = completed.Count,
            PendingCount = pending.Count,
            Pending = pending,
            Completed = completed
        };
    }

    // ─── CREATE ───────────────────────────────────────────────────────────────

    public async Task<ChecklistItemDto?> CreateChecklistItemAsync(
        int eventId,
        int userId,
        CreateChecklistItemRequest request)
    {
        var owned = await _eventService.EventBelongsToUserAsync(eventId, userId);
        if (!owned)
            return null;

        var item = new ChecklistItem
        {
            EventId = eventId,
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            Priority = ParsePriorityEnum(request.Priority),
            IsCompleted = false,
            Category = request.Category,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<ChecklistItem>().AddAsync(item);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(item);
    }

    // ─── UPDATE ───────────────────────────────────────────────────────────────

    public async Task<ChecklistItemDto?> UpdateChecklistItemAsync(
        int itemId,
        int userId,
        UpdateChecklistItemRequest request)
    {
        var item = await _unitOfWork
            .Repository<ChecklistItem>()
            .GetByIdAsync(itemId);

        if (item == null)
            return null;

        var owned = await _eventService.EventBelongsToUserAsync(item.EventId, userId);
        if (!owned)
            return null;

        item.Title = request.Title;
        item.Description = request.Description;
        item.DueDate = request.DueDate;
        item.Priority = ParsePriorityEnum(request.Priority);
        item.IsCompleted = request.IsCompleted;
        item.Category = request.Category;
        item.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(item);
    }

    // ─── DELETE ───────────────────────────────────────────────────────────────

    public async Task<bool> DeleteChecklistItemAsync(int itemId, int userId)
    {
        var item = await _unitOfWork
            .Repository<ChecklistItem>()
            .GetByIdAsync(itemId);

        if (item == null)
            return false;

        var owned = await _eventService.EventBelongsToUserAsync(item.EventId, userId);
        if (!owned)
            return false;

        _unitOfWork.Repository<ChecklistItem>().Delete(item);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // ─── TOGGLE ───────────────────────────────────────────────────────────────

    public async Task<ChecklistItemDto?> ToggleChecklistItemAsync(
        int itemId,
        int userId)
    {
        var item = await _unitOfWork
            .Repository<ChecklistItem>()
            .GetByIdAsync(itemId);

        if (item == null)
            return null;

        var owned = await _eventService.EventBelongsToUserAsync(item.EventId, userId);
        if (!owned)
            return null;

        item.IsCompleted = !item.IsCompleted;
        item.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(item);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static ChecklistItemDto MapToDto(ChecklistItem item) => new()
    {
        Id = item.Id,
        EventId = item.EventId,
        Title = item.Title,
        Description = item.Description,
        DueDate = item.DueDate,
        Priority = item.Priority.ToString(),
        IsCompleted = item.IsCompleted,
        Category = item.Category,
        CreatedAt = item.CreatedAt,
        UpdatedAt = item.UpdatedAt
    };

    /// <summary>Returns the numeric value of the priority for descending sort.</summary>
    private static int ParsePriority(string priority) =>
        Enum.TryParse<TaskPriority>(priority, ignoreCase: true, out var p)
            ? (int)p
            : (int)TaskPriority.Medium;

    private static TaskPriority ParsePriorityEnum(string priority) =>
        Enum.TryParse<TaskPriority>(priority, ignoreCase: true, out var p)
            ? p
            : TaskPriority.Medium;
}