using EventHub.Domain.Enums;

namespace EventHub.Application.DTOs.Payment;

/// <summary>One row of GET /api/admin/payments — the Global Payment Ledger.</summary>
public class AdminPaymentLedgerItemDto
{
    public int PaymentId { get; set; }

    public int BookingId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string VendorName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus Status { get; set; }

    public DateTime Timestamp { get; set; }
}

/// <summary>GET /api/admin/payments/kpis.</summary>
public class AdminPaymentKpisDto
{
    public decimal TotalRevenue { get; set; }

    public decimal TotalPlatformFees { get; set; }

    public int TotalTransactions { get; set; }

    public double RefundRate { get; set; }

    public double FailedTransactionRate { get; set; }
}

/// <summary>POST /api/admin/payments/{id}/refund body — Refund is an admin-only manual decision (no automation in MVP).</summary>
public class IssueRefundRequestDto
{
    public string? Reason { get; set; }
}
