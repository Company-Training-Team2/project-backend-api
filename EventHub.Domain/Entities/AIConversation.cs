using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

/// <summary>
/// Represents an AI chat conversation linked to a specific event.
/// A single event can contain multiple conversations in the future.
/// </summary>
public class AIConversation : SoftDeletableEntity
{
    /// <summary>
    /// Related Event Id.
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// Conversation title displayed in history.
    /// Example:
    /// "Budget Planning"
    /// "Wedding Ideas"
    /// "Vendor Suggestions"
    /// </summary>
    public string Title { get; set; } = "New Conversation";

    /// <summary>
    /// Navigation Property.
    /// </summary>
    public Event Event { get; set; } = null!;

    /// <summary>
    /// Conversation messages.
    /// </summary>
    public ICollection<AIMessage> Messages { get; set; } = new List<AIMessage>();
}