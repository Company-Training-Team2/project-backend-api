using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

/// <summary>
/// Internal platform CRM thread between admin and a user.
/// GET /POST /api/admin/conversations.
/// </summary>
public class AdminConversation : BaseEntity
{
    public int UserId { get; set; }
    public string Subject { get; set; } = string.Empty;

    /// <summary>Open | Resolved | Closed</summary>
    public string Status { get; set; } = "Open";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ────────────────────────────────────────────────────────────
    public User User { get; set; } = null!;
    public ICollection<AdminConversationMessage> Messages { get; set; } = new List<AdminConversationMessage>();
}

/// <summary>Individual message within a CRM conversation thread.</summary>
public class AdminConversationMessage : BaseEntity
{
    public int ConversationId { get; set; }

    /// <summary>Null = sent by admin; set = sent by the user.</summary>
    public int? SenderUserId { get; set; }

    public string Body { get; set; } = string.Empty;
    public bool IsReadByUser { get; set; } = false;
    public bool IsReadByAdmin { get; set; } = false;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ────────────────────────────────────────────────────────────
    public AdminConversation Conversation { get; set; } = null!;
}
