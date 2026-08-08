namespace EventHub.Application.DTOs.Expense;

/// <summary>
/// POST /api/events/{id}/expenses — manual out-of-pocket entry
/// (gifts, local transport, etc). System-generated entries from confirmed
/// bookings are created internally by PaymentService and cannot be created
/// through this endpoint.
/// </summary>
public class CreateExpenseRequest
{
    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    /// <summary>Paid / Pending / Flagged. Defaults to Pending when omitted.</summary>
    public string? Status { get; set; }

    public DateTime? Date { get; set; }
}
