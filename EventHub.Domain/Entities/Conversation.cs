using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

/// <summary>
/// Direct messaging thread between one Customer-role user and one
/// Vendor-role user — the "Contact Vendor" feature. Distinct from
/// AdminConversation (admin&lt;-&gt;any-user support/CRM threads); this is
/// peer-to-peer between the two marketplace sides. Optionally anchored to
/// the WorkPost the customer was viewing when they started the thread, so
/// the UI can show what it's about — not required (a thread can outlive
/// the listing it started from).
/// </summary>
public class Conversation : BaseEntity
{
    public int CustomerUserId { get; set; }
    public int VendorUserId { get; set; }
    public int? WorkPostId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ───────────────────────────────────────────────────────────
    public User CustomerUser { get; set; } = null!;
    public User VendorUser { get; set; } = null!;
    public WorkPost? WorkPost { get; set; }
    public ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
}

/// <summary>Individual message within a Conversation thread.</summary>
public class ConversationMessage : BaseEntity
{
    public int ConversationId { get; set; }
    public int SenderUserId { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsReadByCustomer { get; set; } = false;
    public bool IsReadByVendor { get; set; } = false;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // ── Navigation ───────────────────────────────────────────────────────────
    public Conversation Conversation { get; set; } = null!;
}
