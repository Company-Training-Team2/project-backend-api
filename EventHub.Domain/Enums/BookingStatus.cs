namespace EventHub.Domain.Enums;

/// <summary>
/// PRD-mandated flow: Pending -> Accepted -> Paid -> Completed, with
/// Cancelled / Rejected as terminal off-ramps. Renamed from the original
/// "Confirmed" to "Accepted" to align exactly with the PRD wording
/// (audit Module 8 action item).
/// </summary>
public enum BookingStatus
{
    Pending = 1,
    Accepted = 2,
    Completed = 3,
    Cancelled = 4,
    Rejected = 5,
    Paid = 6
}
