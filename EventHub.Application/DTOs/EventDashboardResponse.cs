namespace EventHub.Application.DTOs;

/// <summary>
/// Aggregated response backing the Event Dashboard widgets:
/// Countdown, Budget, Task Velocity, Guest RSVP.
/// </summary>
public class EventDashboardResponse
{
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Countdown widget. Can be negative if TargetDate has passed.</summary>
    public int DaysUntilEvent { get; set; }

    // ─── Budget widget ──────────────────────────────────────────────────────
    public decimal TotalBudget { get; set; }
    public decimal SpentBudget { get; set; }
    public decimal RemainingBudget { get; set; }

    // ─── Task Velocity widget (e.g. "32/48") — from ChecklistItem sub-system ──
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }

    // ─── Guest RSVP widget (e.g. "Confirmed 124 / Pending 42 / Declined 8") ──
    public int ConfirmedGuests { get; set; }
    public int PendingGuests { get; set; }
    public int DeclinedGuests { get; set; }
}
