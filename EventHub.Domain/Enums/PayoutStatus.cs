namespace EventHub.Domain.Enums;

/// <summary>Status of a vendor Payout record (Payment module).</summary>
public enum PayoutStatus
{
    Pending = 1,
    Processed = 2,
    Failed = 3
}
