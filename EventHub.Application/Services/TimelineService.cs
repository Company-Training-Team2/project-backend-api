using EventHub.Application.DTOs.Timeline;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;

namespace EventHub.Application.Services;

/// <summary>
/// Module 6 – Dynamic milestone timeline.
///
/// Milestones are computed from current Event / Booking / Payment aggregates —
/// no dedicated DB state table is needed (per the spec recommendation).
///
/// Milestone order:
///   1. planning_started    – Always complete once the event exists.
///   2. vendor_booked       – At least one Booking in Accepted / Paid / Completed state.
///   3. invitation_sent     – At least one Guest record exists.
///   4. payments_deposits   – At least one Payment with PaymentStatus.Completed.
///   5. final_confirmation  – All active bookings are Accepted or Paid, AND
///                            ≥ 75 % of checklist tasks are complete.
///   6. event_day           – TargetDate (UTC date) ≤ today.
///   7. completed           – TargetDate (UTC date) < today (event has passed).
/// </summary>
public class TimelineService : ITimelineService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventRepository _eventRepository;
    private readonly IEventService _eventService;

    public TimelineService(
        IUnitOfWork unitOfWork,
        IEventRepository eventRepository,
        IEventService eventService)
    {
        _unitOfWork = unitOfWork;
        _eventRepository = eventRepository;
        _eventService = eventService;
    }

    public async Task<TimelineResponse?> GetTimelineAsync(int eventId, int userId)
    {
        var owned = await _eventService.EventBelongsToUserAsync(eventId, userId);
        if (!owned)
            return null;

        // Load event with all required navigation data.
        var evt = await _eventRepository.GetByIdWithDashboardDataAsync(eventId);
        if (evt == null)
            return null;

        // Load bookings with payments.
        var bookings = (await _unitOfWork
            .Repository<Booking>()
            .FindAsync(b => b.EventId == eventId)).ToList();

        var payments = (await _unitOfWork
            .Repository<Payment>()
            .FindAsync(p => bookings.Select(b => b.Id).Contains(p.BookingId))).ToList();

        var guests = evt.Guests.ToList();
        var tasks = evt.ChecklistItems.ToList();
        var today = DateTime.UtcNow.Date;

        // ── Milestone evaluation ─────────────────────────────────────────────

        // 1. Planning Started — always true once event exists.
        var planningStarted = true;
        var planningDate = evt.CreatedAt;

        // 2. Vendor Booked — at least one booking accepted/paid/completed.
        var activeBookings = bookings
            .Where(b => b.Status is BookingStatus.Accepted
                               or BookingStatus.Paid
                               or BookingStatus.Completed)
            .ToList();

        var vendorBooked = activeBookings.Any();
        var firstBookingDate = activeBookings
            .Select(b => b.CreatedAt)
            .OrderBy(d => d)
            .FirstOrDefault();

        // 3. Invitation Sent — at least one guest record.
        var invitationSent = guests.Any();
        var firstGuestDate = guests
            .Select(g => g.CreatedAt)
            .OrderBy(d => d)
            .FirstOrDefault();

        // 4. Payments & Deposits — at least one completed payment.
        var completedPayments = payments
            .Where(p => p.PaymentStatus == PaymentStatus.Paid)
            .ToList();

        var paymentsDeposited = completedPayments.Any();
        var firstPaymentDate = completedPayments
            .Select(p => p.PaidAt)
            .OrderBy(d => d)
            .FirstOrDefault();

        // 5. Final Confirmation — all active bookings confirmed/paid AND ≥ 75 % tasks done.
        var allBookingsConfirmed = activeBookings.Any() &&
            activeBookings.All(b => b.Status is BookingStatus.Accepted or BookingStatus.Paid);

        var taskCompletionRatio = tasks.Count == 0
            ? 1.0
            : (double)tasks.Count(t => t.IsCompleted) / tasks.Count;

        var finalConfirmed = allBookingsConfirmed && taskCompletionRatio >= 0.75;

        // 6. Event Day — TargetDate (date part) ≤ today.
        var isEventDay = evt.TargetDate.Date <= today;

        // 7. Completed — TargetDate (date part) < today (fully in the past).
        var isCompleted = evt.TargetDate.Date < today;

        var milestones = new List<TimelineMilestoneDto>
        {
            new()
            {
                Key         = "planning_started",
                Label       = "Planning Started",
                IsCompleted = planningStarted,
                CompletedAt = planningStarted ? planningDate : null,
                Description = "Event created and planning is underway."
            },
            new()
            {
                Key         = "vendor_booked",
                Label       = "Vendor Booked",
                IsCompleted = vendorBooked,
                CompletedAt = vendorBooked ? firstBookingDate : null,
                Description = "At least one vendor booking has been confirmed."
            },
            new()
            {
                Key         = "invitation_sent",
                Label       = "Invitation Sent",
                IsCompleted = invitationSent,
                CompletedAt = invitationSent ? firstGuestDate : null,
                Description = "Guest list has been created and invitations are out."
            },
            new()
            {
                Key         = "payments_deposits",
                Label       = "Payments & Deposits",
                IsCompleted = paymentsDeposited,
                CompletedAt = paymentsDeposited ? firstPaymentDate : null,
                Description = "At least one deposit or payment has been completed."
            },
            new()
            {
                Key         = "final_confirmation",
                Label       = "Final Confirmation",
                IsCompleted = finalConfirmed,
                CompletedAt = finalConfirmed ? (DateTime?)DateTime.UtcNow : null,
                Description = "All vendors confirmed and 75 % or more of tasks completed."
            },
            new()
            {
                Key         = "event_day",
                Label       = "Event Day",
                IsCompleted = isEventDay,
                CompletedAt = isEventDay ? evt.TargetDate.Date : null,
                Description = "The event date has arrived."
            },
            new()
            {
                Key         = "completed",
                Label       = "Completed",
                IsCompleted = isCompleted,
                CompletedAt = isCompleted ? evt.TargetDate.Date.AddDays(1) : null,
                Description = "The event has concluded successfully."
            }
        };

        return new TimelineResponse
        {
            EventId = eventId,
            EventName = evt.Name,
            Milestones = milestones
        };
    }
}