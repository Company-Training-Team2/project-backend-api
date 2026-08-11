namespace EventHub.Application.DTOs.Admin;

/// <summary>
/// GET /api/admin/conversations
/// Internal platform CRM — admin-to-user messaging thread list.
/// </summary>
public class AdminConversationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string? UserDisplayName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;      // Open | Resolved | Closed
    public string? LastMessageSnippet { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int UnreadCount { get; set; }
}

/// <summary>POST /api/admin/conversations — open a new CRM thread.</summary>
public class CreateAdminConversationDto
{
    public int UserId { get; set; }
    public string Subject { get; set; } = string.Empty;
    /// <summary>Optional opening message body sent along with the thread creation.</summary>
    public string? InitialMessage { get; set; }
}

/// <summary>GET /api/admin/conversations/{id}/messages — full thread for one CRM conversation.</summary>
public class AdminConversationMessageDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    /// <summary>Null = sent by admin; set = sent by the user.</summary>
    public int? SenderUserId { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsReadByUser { get; set; }
    public bool IsReadByAdmin { get; set; }
}

/// <summary>POST /api/admin/conversations/{id}/messages — admin reply.</summary>
public class SendAdminConversationMessageDto
{
    public string Body { get; set; } = string.Empty;
}

/// <summary>PATCH /api/admin/conversations/{id}/status.</summary>
public class UpdateConversationStatusDto
{
    public string Status { get; set; } = string.Empty; // Open | Resolved | Closed
}
