namespace EventHub.Application.DTOs.Messaging;

/// <summary>
/// GET /api/messaging/conversations — one row per thread, from the calling
/// user's point of view (works the same whether the caller is the
/// Customer or the Vendor side of the thread).
/// </summary>
public class ConversationDto
{
    public int Id { get; set; }
    public int OtherPartyUserId { get; set; }
    public string OtherPartyName { get; set; } = string.Empty;
    public string OtherPartyRole { get; set; } = string.Empty; // "Customer" | "Vendor"
    public int? WorkPostId { get; set; }
    public string? WorkPostTitle { get; set; }
    public string? LastMessageSnippet { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int UnreadCount { get; set; }
}

/// <summary>
/// POST /api/messaging/conversations — a Customer starts (or resumes) a
/// thread with the vendor behind a WorkPost. Vendors reply through an
/// existing thread rather than starting new ones (see
/// SendMessage/GetMessages) — there's no "vendor cold-messages a
/// customer" flow.
/// </summary>
public class CreateConversationDto
{
    public int WorkPostId { get; set; }
    public string? InitialMessage { get; set; }
}

public class ConversationMessageDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public int SenderUserId { get; set; }
    public bool IsFromMe { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}

public class SendConversationMessageDto
{
    public string Body { get; set; } = string.Empty;
}
