using System.Security.Claims;
using EventHub.Application.DTOs.Notification;
using EventHub.Application.DTOs.Payment;
using EventHub.Application.DTOs.Vendor;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Application.Services;

/// <summary>
/// Payment module (Paymob integration):
///  - POST /api/payments/checkout/{bookingId} -> InitiateCheckoutAsync
///  - POST /api/payments/webhook              -> HandleGatewayCallbackAsync
///  - GET  /api/payments/my                   -> GetMyPaymentsAsync
///  - GET  /api/payments/{bookingId}           -> GetByBookingIdAsync
///  - GET  /api/vendor/earnings                -> GetVendorEarningsAsync
///  - Admin ledger / refund / KPIs.
///
/// Platform commission is a fixed 10% on every booking, snapshotted onto the
/// Payment at checkout time so later policy changes never affect payments
/// already made. All money is collected into the platform account first;
/// vendor payouts are only created once the booking is Completed (see
/// PayoutService), never at payment time.
/// </summary>
public class PaymentService : IPaymentService
{
    private const decimal PlatformCommissionRate = 0.10m;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGateway _paymentGateway;
    private readonly INotificationService _notificationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PaymentService(
        IUnitOfWork unitOfWork,
        IPaymentGateway paymentGateway,
        INotificationService notificationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _paymentGateway = paymentGateway;
        _notificationService = notificationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<CheckoutResultDto> InitiateCheckoutAsync(int bookingId)
    {
        var profile = await GetCurrentCustomerProfileAsync();

        var booking = await _unitOfWork.Repository<Booking>()
            .Query()
            .Include(b => b.Customer)
                .ThenInclude(c => c.User)
            .Include(b => b.WorkPost)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null)
            throw new Exception("Booking not found.");

        if (booking.CustomerId != profile.Id)
            throw new UnauthorizedAccessException("You do not own this booking.");

        if (booking.Status != BookingStatus.Confirmed)
            throw new Exception("Payment can only be initiated for a confirmed booking.");

        var grossAmount = booking.TotalPrice;
        var platformFee = Math.Round(grossAmount * PlatformCommissionRate, 2);
        var vendorPayout = grossAmount - platformFee;

        var payment = booking.Payment;

        if (payment is null)
        {
            payment = new Payment
            {
                BookingId = booking.Id,
                // PaymentMethod is a required field on the entity; the actual
                // method used is only known once Paymob's callback arrives
                // (see InferPaymentMethod), so this is a placeholder until then.
                PaymentMethod = PaymentMethod.Visa,
                PaymentStatus = PaymentStatus.Pending,
                PaymentGateway = "Paymob",
                GrossAmount = grossAmount,
                CommissionRateSnapshot = PlatformCommissionRate,
                PlatformFeeAmount = platformFee,
                VendorPayoutAmount = vendorPayout
            };

            await _unitOfWork.Repository<Payment>().AddAsync(payment);
        }
        else
        {
            if (payment.PaymentStatus == PaymentStatus.Paid)
                throw new Exception("This booking has already been paid.");

            if (payment.PaymentStatus == PaymentStatus.Refunded)
                throw new Exception("This booking's payment was refunded and cannot be retried.");

            // Pending or Failed: refresh the snapshot (in case TotalPrice
            // changed) and let the customer retry checkout.
            payment.GrossAmount = grossAmount;
            payment.CommissionRateSnapshot = PlatformCommissionRate;
            payment.PlatformFeeAmount = platformFee;
            payment.VendorPayoutAmount = vendorPayout;
            payment.PaymentStatus = PaymentStatus.Pending;

            _unitOfWork.Repository<Payment>().Update(payment);
        }

        // Save first so a newly-created Payment has an Id to embed in the
        // gateway's merchant_order_id (needed to map the webhook back to it).
        await _unitOfWork.SaveChangesAsync();

        var (firstName, lastName) = SplitName(booking.Customer.FullName);

        var gatewayResult = await _paymentGateway.CreatePaymentKeyAsync(new PaymentGatewayRequest
        {
            BookingId = booking.Id,
            PaymentId = payment.Id,
            AmountEgp = grossAmount,
            CustomerFirstName = firstName,
            CustomerLastName = lastName,
            CustomerEmail = booking.Customer.User.Email ?? "customer@eventhub.com",
            CustomerPhone = booking.Customer.PhoneNumber
        });

        if (!gatewayResult.Success)
            throw new Exception($"Payment gateway error: {gatewayResult.ErrorMessage}");

        payment.TransactionId = gatewayResult.GatewayOrderId?.ToString();

        _unitOfWork.Repository<Payment>().Update(payment);

        await _unitOfWork.SaveChangesAsync();

        return new CheckoutResultDto
        {
            PaymentId = payment.Id,
            BookingId = booking.Id,
            GrossAmount = grossAmount,
            PlatformFeeAmount = platformFee,
            VendorPayoutAmount = vendorPayout,
            PaymentStatus = payment.PaymentStatus,
            CheckoutUrl = gatewayResult.CheckoutUrl ?? string.Empty
        };
    }

    public async Task HandleGatewayCallbackAsync(PaymobTransactionCallbackDto callback, string receivedHmac)
    {
        if (!_paymentGateway.VerifyWebhookSignature(receivedHmac, callback))
            throw new Exception("Invalid webhook signature.");

        var merchantOrderId = callback.Order.MerchantOrderId ?? callback.MerchantOrderId;

        if (!TryParsePaymentId(merchantOrderId, out var paymentId))
            throw new Exception("Could not resolve payment from webhook payload.");

        var payment = await _unitOfWork.Repository<Payment>()
            .Query()
            .Include(p => p.Booking)
                .ThenInclude(b => b.Customer)
            .Include(p => p.Booking)
                .ThenInclude(b => b.WorkPost)
                    .ThenInclude(w => w.VendorProfile)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment is null)
            throw new Exception("Payment not found for callback.");

        // Idempotency: Paymob may resend the same callback more than once.
        if (payment.PaymentStatus == PaymentStatus.Paid)
            return;

        var booking = payment.Booking;

        if (callback.Success && !callback.Pending)
        {
            payment.PaymentStatus = PaymentStatus.Paid;
            payment.PaidAt = DateTime.UtcNow;
            payment.TransactionId = callback.Id.ToString();
            payment.PaymentMethod = InferPaymentMethod(callback.SourceData);

            _unitOfWork.Repository<Payment>().Update(payment);

            booking.Status = BookingStatus.Paid;
            _unitOfWork.Repository<Booking>().Update(booking);

            await UpsertBookingExpenseAsync(booking, payment.GrossAmount);

            await _unitOfWork.SaveChangesAsync();

            await NotifyAsync(
                booking.Customer.UserId,
                "Payment received",
                $"Your payment for booking #{booking.Id} was received successfully.",
                booking.Id);

            if (booking.WorkPost is not null)
            {
                await NotifyAsync(
                    booking.WorkPost.VendorProfile.UserId,
                    "Booking paid",
                    $"Booking #{booking.Id} has been paid by the customer.",
                    booking.Id);
            }
        }
        else
        {
            // Payment failed: leave the booking Confirmed so the customer can retry checkout.
            payment.PaymentStatus = PaymentStatus.Failed;
            _unitOfWork.Repository<Payment>().Update(payment);

            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<PaymentDto>> GetMyPaymentsAsync()
    {
        var profile = await GetCurrentCustomerProfileAsync();

        var payments = await _unitOfWork.Repository<Payment>()
            .Query()
            .Where(p => p.Booking.CustomerId == profile.Id)
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        return payments.Select(MapToDto);
    }

    public async Task<PaymentDto> GetByBookingIdAsync(int bookingId)
    {
        var profile = await GetCurrentCustomerProfileAsync();

        var payment = await _unitOfWork.Repository<Payment>()
            .FirstOrDefaultAsync(p => p.BookingId == bookingId);

        if (payment is null)
            throw new Exception("Payment not found.");

        var booking = await _unitOfWork.Repository<Booking>().GetByIdAsync(bookingId);

        if (booking is null || booking.CustomerId != profile.Id)
            throw new UnauthorizedAccessException("You do not own this booking.");

        return MapToDto(payment);
    }

    public async Task<VendorEarningsDto> GetVendorEarningsAsync(int userId)
    {
        var vendor = await _unitOfWork.Repository<VendorProfile>()
            .FirstOrDefaultAsync(v => v.UserId == userId);

        if (vendor is null)
            throw new Exception("Vendor profile not found.");

        var payments = await _unitOfWork.Repository<Payment>()
            .Query()
            .Where(p => p.Booking.WorkPost.VendorProfileId == vendor.Id && p.PaymentStatus == PaymentStatus.Paid)
            .Include(p => p.Payout)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync();

        var result = new VendorEarningsDto
        {
            TotalEarnings = payments.Sum(p => p.VendorPayoutAmount),
            PendingPayoutAmount = payments
                .Where(p => p.Payout is null || p.Payout.Status == PayoutStatus.Pending)
                .Sum(p => p.VendorPayoutAmount),
            ProcessedPayoutAmount = payments
                .Where(p => p.Payout is not null && p.Payout.Status == PayoutStatus.Processed)
                .Sum(p => p.VendorPayoutAmount),
            Items = payments.Select(p => new VendorEarningItemDto
            {
                BookingId = p.BookingId,
                PaymentId = p.Id,
                GrossAmount = p.GrossAmount,
                PlatformFeeAmount = p.PlatformFeeAmount,
                VendorPayoutAmount = p.VendorPayoutAmount,
                PaymentStatus = p.PaymentStatus,
                PaidAt = p.PaidAt
            }).ToList()
        };

        return result;
    }

    // ───────────────── Admin ─────────────────

    public async Task<IEnumerable<AdminPaymentLedgerItemDto>> GetPaymentLedgerAsync(string? status, int page, int pageSize)
    {
        var query = _unitOfWork.Repository<Payment>()
            .Query()
            .Include(p => p.Booking)
                .ThenInclude(b => b.Customer)
            .Include(p => p.Booking)
                .ThenInclude(b => b.WorkPost)
                    .ThenInclude(w => w.VendorProfile)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatus>(status, true, out var parsedStatus))
            query = query.Where(p => p.PaymentStatus == parsedStatus);

        var payments = await query
            .OrderByDescending(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return payments.Select(p => new AdminPaymentLedgerItemDto
        {
            PaymentId = p.Id,
            BookingId = p.BookingId,
            CustomerName = p.Booking.Customer.FullName,
            VendorName = p.Booking.WorkPost.VendorProfile.BusinessName,
            Amount = p.GrossAmount,
            PaymentMethod = p.PaymentMethod,
            Status = p.PaymentStatus,
            // Payment has no CreatedAt of its own — PaidAt is the most
            // meaningful timestamp once settled, falling back to when the
            // booking was created for payments still Pending/Failed.
            Timestamp = p.PaidAt ?? p.Booking.CreatedAt
        });
    }

    public async Task<PaymentDto> IssueRefundAsync(int paymentId, IssueRefundRequestDto dto)
    {
        var payment = await _unitOfWork.Repository<Payment>()
            .Query()
            .Include(p => p.Booking)
                .ThenInclude(b => b.Customer)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment is null)
            throw new Exception("Payment not found.");

        if (payment.PaymentStatus != PaymentStatus.Paid)
            throw new Exception("Only paid payments can be refunded.");

        payment.PaymentStatus = PaymentStatus.Refunded;

        _unitOfWork.Repository<Payment>().Update(payment);

        await _unitOfWork.SaveChangesAsync();

        var reasonSuffix = string.IsNullOrWhiteSpace(dto.Reason) ? string.Empty : $" Reason: {dto.Reason}";

        await NotifyAsync(
            payment.Booking.Customer.UserId,
            "Refund issued",
            $"Your payment for booking #{payment.BookingId} has been refunded.{reasonSuffix}",
            payment.BookingId);

        return MapToDto(payment);
    }

    public async Task<AdminPaymentKpisDto> GetPaymentKpisAsync()
    {
        var payments = await _unitOfWork.Repository<Payment>().GetAllAsync();
        var list = payments.ToList();

        var total = list.Count;

        if (total == 0)
            return new AdminPaymentKpisDto();

        var paid = list.Where(p => p.PaymentStatus == PaymentStatus.Paid).ToList();
        var refundedCount = list.Count(p => p.PaymentStatus == PaymentStatus.Refunded);
        var failedCount = list.Count(p => p.PaymentStatus == PaymentStatus.Failed);

        return new AdminPaymentKpisDto
        {
            TotalRevenue = paid.Sum(p => p.GrossAmount),
            TotalPlatformFees = paid.Sum(p => p.PlatformFeeAmount),
            TotalTransactions = total,
            RefundRate = (double)refundedCount / total,
            FailedTransactionRate = (double)failedCount / total
        };
    }

    // ───────────────── Helpers ─────────────────

    private async Task UpsertBookingExpenseAsync(Booking booking, decimal amount)
    {
        var expense = await _unitOfWork.Repository<Expense>()
            .FirstOrDefaultAsync(e => e.BookingId == booking.Id);

        if (expense is null)
        {
            expense = new Expense
            {
                EventId = booking.EventId,
                BookingId = booking.Id,
                Category = "Vendor Booking",
                Description = booking.WorkPost?.Title ?? $"Booking #{booking.Id}",
                Amount = amount,
                Status = ExpenseStatus.Paid,
                Date = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Expense>().AddAsync(expense);
        }
        else
        {
            expense.Amount = amount;
            expense.Status = ExpenseStatus.Paid;
            expense.Date = DateTime.UtcNow;

            _unitOfWork.Repository<Expense>().Update(expense);
        }
    }

    private async Task NotifyAsync(int userId, string title, string body, int bookingId)
    {
        try
        {
            await _notificationService.NotifyAsync(new CreateNotificationDto
            {
                UserId = userId,
                Type = NotificationType.PaymentReceipt,
                Title = title,
                Body = body,
                RelatedEntityId = bookingId
            });
        }
        catch
        {
            // Best-effort: a notification failure must not roll back a successful payment update.
        }
    }

    private static PaymentMethod InferPaymentMethod(PaymobSourceDataDto? sourceData)
    {
        if (sourceData is null)
            return PaymentMethod.Visa;

        var subType = sourceData.SubType?.ToLowerInvariant() ?? string.Empty;
        var type = sourceData.Type?.ToLowerInvariant() ?? string.Empty;

        if (subType.Contains("mastercard"))
            return PaymentMethod.MasterCard;

        if (type.Contains("wallet"))
            return PaymentMethod.VodafoneCash;

        return PaymentMethod.Visa;
    }

    private static (string FirstName, string LastName) SplitName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return ("Customer", "EventHub");

        var parts = fullName.Trim().Split(' ', 2);

        return parts.Length == 2 ? (parts[0], parts[1]) : (parts[0], "-");
    }

    private static bool TryParsePaymentId(string? merchantOrderId, out int paymentId)
    {
        paymentId = 0;

        if (string.IsNullOrWhiteSpace(merchantOrderId))
            return false;

        var segments = merchantOrderId.Split('-');

        return segments.Length == 3 && int.TryParse(segments[2], out paymentId);
    }

    private async Task<CustomerProfile> GetCurrentCustomerProfileAsync()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User is not authenticated.");

        var profile = await _unitOfWork.Repository<CustomerProfile>()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null)
            throw new Exception("Customer profile not found.");

        return profile;
    }

    private static PaymentDto MapToDto(Payment p) => new()
    {
        Id = p.Id,
        BookingId = p.BookingId,
        GrossAmount = p.GrossAmount,
        CommissionRateSnapshot = p.CommissionRateSnapshot,
        PlatformFeeAmount = p.PlatformFeeAmount,
        VendorPayoutAmount = p.VendorPayoutAmount,
        PaymentMethod = p.PaymentMethod,
        PaymentStatus = p.PaymentStatus,
        TransactionId = p.TransactionId,
        PaymentGateway = p.PaymentGateway,
        PaidAt = p.PaidAt
    };
}
