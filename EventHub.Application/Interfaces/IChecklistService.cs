using EventHub.Application.DTOs.Checklist;

namespace EventHub.Application.Interfaces;

/// <summary>
/// Module 5 – Checklist task management.
/// All methods accept the ASP.NET Identity userId (JWT NameIdentifier claim).
/// </summary>
public interface IChecklistService
{
    /// <summary>
    /// Returns all checklist items for the event, grouped into Pending /
    /// Completed buckets and sorted by priority (High → Medium → Low).
    /// Returns null when the event is not found or does not belong to the user.
    /// </summary>
    Task<EventChecklistResponse?> GetChecklistAsync(int eventId, int userId);

    /// <summary>
    /// Creates a new checklist item for the specified event.
    /// Returns null when the event is not found or does not belong to the user.
    /// </summary>
    Task<ChecklistItemDto?> CreateChecklistItemAsync(
        int eventId,
        int userId,
        CreateChecklistItemRequest request);

    /// <summary>
    /// Updates all fields of a single checklist item.
    /// Returns null when the item is not found or does not belong to the user's event.
    /// </summary>
    Task<ChecklistItemDto?> UpdateChecklistItemAsync(
        int itemId,
        int userId,
        UpdateChecklistItemRequest request);

    /// <summary>
    /// Deletes a checklist item.
    /// Returns false when the item is not found or does not belong to the user's event.
    /// </summary>
    Task<bool> DeleteChecklistItemAsync(int itemId, int userId);

    /// <summary>
    /// Flips the IsCompleted flag on a single checklist item.
    /// Returns null when the item is not found or does not belong to the user's event.
    /// </summary>
    Task<ChecklistItemDto?> ToggleChecklistItemAsync(int itemId, int userId);
}