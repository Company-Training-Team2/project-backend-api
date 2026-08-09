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
