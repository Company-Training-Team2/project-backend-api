using EventHub.Application.DTOs.Payment;
using EventHub.Application.DTOs.Vendor;

namespace EventHub.Application.Interfaces;

public interface IPaymentService
{
    /// <summary>POST /api/payments/checkout/{bookingId} — Booking must be Accepted and owned by the current customer.</summary>
    Task<CheckoutResultDto> InitiateCheckoutAsync(int bookingId);

    /// <summary>POST /api/payments/webhook — verifies HMAC, updates Payment/Booking/Expense, sends PaymentReceipt.</summary>
    Task HandleGatewayCallbackAsync(PaymobTransactionCallbackDto callback, string receivedHmac);

    /// <summary>GET /api/payments/my — payments for the current customer's bookings.</summary>
    Task<IEnumerable<PaymentDto>> GetMyPaymentsAsync();

    /// <summary>GET /api/payments/{bookingId}.</summary>
    Task<PaymentDto> GetByBookingIdAsync(int bookingId);

    /// <summary>GET /api/vendor/earnings.</summary>
    Task<VendorEarningsDto> GetVendorEarningsAsync(int userId);

    // ───────────────── Admin ─────────────────

    /// <summary>GET /api/admin/payments — Global Payment Ledger.</summary>
    Task<IEnumerable<AdminPaymentLedgerItemDto>> GetPaymentLedgerAsync(string? status, int page, int pageSize);

    /// <summary>POST /api/admin/payments/{id}/refund — Admin-only, no automation.</summary>
    Task<PaymentDto> IssueRefundAsync(int paymentId, IssueRefundRequestDto dto);

    /// <summary>GET /api/admin/payments/kpis.</summary>
    Task<AdminPaymentKpisDto> GetPaymentKpisAsync();
}
