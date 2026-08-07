using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

/// <summary>
/// Represents a single message exchanged between the user and the AI.
/// </summary>
public class AIMessage : BaseEntity
{
    /// <summary>
    /// Related Conversation Id.
    /// </summary>
    public int AIConversationId { get; set; }

    /// <summary>
    /// Indicates whether the sender is the user or the assistant.
    /// </summary>
    public AIMessageSender Sender { get; set; }

    /// <summary>
    /// Message content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Message creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation Property.
    /// </summary>
    public AIConversation AIConversation { get; set; } = null!;
}