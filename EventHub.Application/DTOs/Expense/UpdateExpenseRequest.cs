namespace EventHub.Application.DTOs.Expense;

/// <summary>PUT /api/expenses/{id} — full update of a manual expense entry.</summary>
public class UpdateExpenseRequest
{
    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    /// <summary>Paid / Pending / Flagged.</summary>
    public string Status { get; set; } = string.Empty;

    public DateTime Date { get; set; }
}
