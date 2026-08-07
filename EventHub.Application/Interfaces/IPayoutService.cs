using EventHub.Application.DTOs.Vendor;

namespace EventHub.Application.Interfaces;

public interface IPayoutService
{
    /// <summary>
    /// Finds Completed bookings with a Paid payment and no Payout yet, and
    /// creates one using the VendorPayoutAmount snapshot stored on the Payment.
    /// Intended to run on a schedule (or be triggered manually by an admin).
    /// </summary>
    Task ProcessDuePayoutsAsync();

    /// <summary>GET /api/vendor/payouts.</summary>
    Task<IEnumerable<PayoutDto>> GetMyPayoutsAsync(int userId);
}
