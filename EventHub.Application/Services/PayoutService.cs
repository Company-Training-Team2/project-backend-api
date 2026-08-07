using EventHub.Application.DTOs.Vendor;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Application.Services;

/// <summary>
/// Payment module: turns a completed, paid booking into a due Payout for the
/// vendor. The actual bank transfer is out of scope for this MVP — this only
/// creates the Payout record (Pending) using the VendorPayoutAmount snapshot
/// already stored on the Payment; settling it is a manual/admin follow-up.
/// </summary>
public class PayoutService : IPayoutService
{
    private readonly IUnitOfWork _unitOfWork;

    public PayoutService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task ProcessDuePayoutsAsync()
    {
        var duePayments = await _unitOfWork.Repository<Payment>()
            .Query()
            .Where(p =>
                p.PaymentStatus == PaymentStatus.Paid &&
                p.Booking.Status == BookingStatus.Completed &&
                p.Payout == null)
            .Include(p => p.Booking)
                .ThenInclude(b => b.WorkPost)
            .ToListAsync();

        if (duePayments.Count == 0)
            return;

        foreach (var payment in duePayments)
        {
            var payout = new Payout
            {
                VendorProfileId = payment.Booking.WorkPost.VendorProfileId,
                PaymentId = payment.Id,
                Amount = payment.VendorPayoutAmount,
                Status = PayoutStatus.Pending,
                ProcessedAt = null
            };

            await _unitOfWork.Repository<Payout>().AddAsync(payout);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<PayoutDto>> GetMyPayoutsAsync(int userId)
    {
        var vendor = await _unitOfWork.Repository<VendorProfile>()
            .FirstOrDefaultAsync(v => v.UserId == userId);

        if (vendor is null)
            throw new Exception("Vendor profile not found.");

        var payouts = await _unitOfWork.Repository<Payout>()
            .Query()
            .Where(p => p.VendorProfileId == vendor.Id)
            .Include(p => p.Payment)
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        return payouts.Select(p => new PayoutDto
        {
            Id = p.Id,
            PaymentId = p.PaymentId,
            BookingId = p.Payment.BookingId,
            Amount = p.Amount,
            Status = p.Status,
            ProcessedAt = p.ProcessedAt
        });
    }
}
