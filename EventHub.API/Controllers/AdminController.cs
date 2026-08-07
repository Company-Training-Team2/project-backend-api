using EventHub.Application.DTOs;
using EventHub.Application.DTOs.Payment;
using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IPaymentService _paymentService;
    private readonly IPayoutService _payoutService;

    public AdminController(IAdminService adminService, IPaymentService paymentService, IPayoutService payoutService)
    {
        _adminService = adminService;
        _paymentService = paymentService;
        _payoutService = payoutService;
    }

    // ═══════════════════════════════════════════════════════════
    // Dashboard — GET /api/admin/dashboard
    // ═══════════════════════════════════════════════════════════
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _adminService.GetDashboardAsync();
        return Ok(dashboard);
    }

    // ═══════════════════════════════════════════════════════════
    // Users — GET /api/admin/users
    // ═══════════════════════════════════════════════════════════
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? role,
        [FromQuery] bool? isDeleted,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var users = await _adminService.GetUsersAsync(role, isDeleted, page, pageSize);
        return Ok(users);
    }

    // ── PUT /api/admin/users/{id}/suspend ─────────────────────────────────────
    [HttpPut("users/{id:int}/suspend")]
    public async Task<IActionResult> SuspendUser(int id)
    {
        var success = await _adminService.SuspendUserAsync(id);
        return success
            ? Ok(new { message = "User suspended." })
            : NotFound(new { message = "User not found." });
    }

    // ── PUT /api/admin/users/{id}/activate ────────────────────────────────────
    [HttpPut("users/{id:int}/activate")]
    public async Task<IActionResult> ActivateUser(int id)
    {
        var success = await _adminService.ActivateUserAsync(id);
        return success
            ? Ok(new { message = "User activated." })
            : NotFound(new { message = "User not found." });
    }

    // ═══════════════════════════════════════════════════════════
    // Vendor Approval Queue — GET /api/admin/vendors/pending
    // ═══════════════════════════════════════════════════════════
    [HttpGet("vendors/pending")]
    public async Task<IActionResult> GetPendingVendors()
    {
        var vendors = await _adminService.GetPendingVendorsAsync();
        return Ok(vendors);
    }

    // ── GET /api/admin/vendors ────────────────────────────────────────────────
    [HttpGet("vendors")]
    public async Task<IActionResult> GetAllVendors(
        [FromQuery] string? approvalStatus,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var vendors = await _adminService.GetAllVendorsAsync(approvalStatus, page, pageSize);
        return Ok(vendors);
    }

    // ── PUT /api/admin/vendors/{id}/approve ───────────────────────────────────
    [HttpPut("vendors/{id:int}/approve")]
    public async Task<IActionResult> ApproveVendor(int id, [FromBody] VendorDecisionRequest? request)
    {
        var success = await _adminService.ApproveVendorAsync(id, request?.Reason);
        return success
            ? Ok(new { message = "Vendor approved." })
            : NotFound(new { message = "Vendor not found." });
    }

    // ── PUT /api/admin/vendors/{id}/reject ────────────────────────────────────
    [HttpPut("vendors/{id:int}/reject")]
    public async Task<IActionResult> RejectVendor(int id, [FromBody] VendorDecisionRequest? request)
    {
        var success = await _adminService.RejectVendorAsync(id, request?.Reason);
        return success
            ? Ok(new { message = "Vendor rejected." })
            : NotFound(new { message = "Vendor not found." });
    }

    // ── PUT /api/admin/vendors/{id}/request-changes ───────────────────────────
    [HttpPut("vendors/{id:int}/request-changes")]
    public async Task<IActionResult> RequestVendorChanges(int id, [FromBody] VendorDecisionRequest? request)
    {
        var success = await _adminService.RequestVendorChangesAsync(id, request?.Reason);
        return success
            ? Ok(new { message = "Change request sent to vendor." })
            : NotFound(new { message = "Vendor not found." });
    }

    // ═══════════════════════════════════════════════════════════
    // Payments (Payment module) — Global Payment Ledger
    // ═══════════════════════════════════════════════════════════

    /// <summary>GET /api/admin/payments — Global Payment Ledger, optionally filtered by status (Pending/Paid/Failed/Refunded).</summary>
    [HttpGet("payments")]
    public async Task<IActionResult> GetPaymentLedger(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _paymentService.GetPaymentLedgerAsync(status, page, pageSize);
        return Ok(result);
    }

    /// <summary>POST /api/admin/payments/{id}/refund — Admin-only manual refund decision (no automation in MVP).</summary>
    [HttpPost("payments/{id:int}/refund")]
    public async Task<IActionResult> RefundPayment(int id, [FromBody] IssueRefundRequestDto? dto)
    {
        var result = await _paymentService.IssueRefundAsync(id, dto ?? new IssueRefundRequestDto());
        return Ok(result);
    }

    /// <summary>GET /api/admin/payments/kpis — total revenue, refund rate, failed transaction rate.</summary>
    [HttpGet("payments/kpis")]
    public async Task<IActionResult> GetPaymentKpis()
    {
        var result = await _paymentService.GetPaymentKpisAsync();
        return Ok(result);
    }

    /// <summary>POST /api/admin/payouts/process — manual fallback trigger for due vendor Payouts (Completed + Paid bookings with no Payout yet). Idempotent.</summary>
    [HttpPost("payouts/process")]
    public async Task<IActionResult> ProcessDuePayouts()
    {
        await _payoutService.ProcessDuePayoutsAsync();
        return Ok(new { message = "Due payouts processed." });
    }
}
