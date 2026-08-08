namespace EventHub.Application.DTOs.Expense;

/// <summary>
/// Module 4 – single ledger entry (either auto-generated from a confirmed
/// booking via Payment, or manually entered by the customer).
/// </summary>
public class ExpenseDto
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    /// <summary>Paid / Pending / Flagged.</summary>
    public string Status { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    /// <summary>Set when this entry was auto-generated from a confirmed vendor booking; null for manual entries.</summary>
    public int? BookingId { get; set; }

    /// <summary>True when BookingId is set — the frontend should disable edit/delete for system-generated rows.</summary>
    public bool IsSystemGenerated => BookingId.HasValue;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
