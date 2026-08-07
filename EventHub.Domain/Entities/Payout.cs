using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

/// <summary>
/// Payment module: represents a due/settled transfer of a vendor's earnings
/// (Payment.VendorPayoutAmount) to their bank account. Created by
/// IPayoutService.ProcessDuePayoutsAsync once the related Booking reaches
/// Completed — actual money never moves at payment time, it sits on the
/// platform account until the event is completed.
/// </summary>
public class Payout : AuditableEntity
{
    public int VendorProfileId { get; set; }

    public int PaymentId { get; set; }

    public decimal Amount { get; set; }

    public PayoutStatus Status { get; set; } = PayoutStatus.Pending;

    public DateTime? ProcessedAt { get; set; }

    // ─── Navigation Properties ────────────────────────────────────────────────
    public VendorProfile VendorProfile { get; set; } = null!;

    public Payment Payment { get; set; } = null!;
}
