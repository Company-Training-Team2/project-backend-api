namespace EventHub.Application.DTOs.Expense;

/// <summary>
/// GET /api/events/{id}/budget — Module 4 "Budget metrics summary: Total,
/// Spent, Remaining, and Category Breakdown".
/// Spent = sum of Expenses with Status == Paid (Pending/Flagged are not
/// counted as spent yet, matching the existing EventDashboardResponse rule).
/// </summary>
public class BudgetSummaryDto
{
    public int EventId { get; set; }

    public decimal TotalBudget { get; set; }

    public decimal SpentBudget { get; set; }

    public decimal RemainingBudget { get; set; }

    /// <summary>Sum of Pending + Flagged expenses — not yet counted as spent, but committed/at-risk.</summary>
    public decimal PendingBudget { get; set; }

    public List<CategoryBreakdownItemDto> CategoryBreakdown { get; set; } = new();
}

public class CategoryBreakdownItemDto
{
    public string Category { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    /// <summary>Percentage of SpentBudget this category represents (0-100).</summary>
    public double Percentage { get; set; }
}
