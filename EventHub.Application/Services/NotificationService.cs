using System.Security.Claims;
using EventHub.Application.DTOs.Notification;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EventHub.Application.Services;

/// <summary>
/// Audit Module 10 (Notifications): event-driven inbox for booking status
/// updates, vendor matches, security alerts, payment receipts, reviews and
/// messages, grouped chronologically. Persists via EF and pushes in real time
/// through INotificationPublisher (SignalR Hub, wired in the API layer).
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly INotificationPublisher _publisher;

    public NotificationService(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        INotificationPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _publisher = publisher;
    }

    public async Task<NotificationDto> NotifyAsync(CreateNotificationDto dto)
    {
        var notification = new Notification
        {
            UserId = dto.UserId,
            Type = dto.Type,
            Title = dto.Title,
            Body = dto.Body,
            RelatedEntityId = dto.RelatedEntityId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Notification>().AddAsync(notification);

        await _unitOfWork.SaveChangesAsync();

        var result = MapToDto(notification);

        // Real-time push is best-effort: the notification is already durable in
        // the database, so a delivery failure here shouldn't roll anything back —
        // the client will still see it on the next GET /api/notifications.
        try
        {
            await _publisher.PublishAsync(dto.UserId, result);
        }
        catch
        {
            // Swallow: SignalR delivery failure must not fail the triggering action
            // (e.g. a booking accept) that raised this notification.
        }

        return result;
    }

    public async Task<NotificationFeedDto> GetFeedAsync()
    {
        var userId = GetCurrentUserId();

        var notifications = (await _unitOfWork.Repository<Notification>()
            .FindAsync(n => n.UserId == userId))
            .OrderByDescending(n => n.CreatedAt)
            .ToList();

        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);

        var feed = new NotificationFeedDto
        {
            UnreadCount = notifications.Count(n => !n.IsRead)
        };

        foreach (var n in notifications)
        {
            var dto = MapToDto(n);

            var bucket = n.CreatedAt.Date == today
                ? feed.Today
                : n.CreatedAt.Date == yesterday
                    ? feed.Yesterday
                    : feed.Earlier;

            bucket.Add(dto);
        }

        return feed;
    }

    public async Task<NotificationDto> MarkAsReadAsync(int notificationId)
    {
        var userId = GetCurrentUserId();

        var notification = await _unitOfWork.Repository<Notification>().GetByIdAsync(notificationId);

        if (notification is null || notification.UserId != userId)
            throw new Exception("Notification not found.");

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Notification>().Update(notification);

            await _unitOfWork.SaveChangesAsync();
        }

        return MapToDto(notification);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User is not authenticated.");

        return userId;
    }

    private static NotificationDto MapToDto(Notification n)
    {
        return new NotificationDto
        {
            Id = n.Id,
            Type = n.Type,
            Title = n.Title,
            Body = n.Body,
            IsRead = n.IsRead,
            RelatedEntityId = n.RelatedEntityId,
            CreatedAt = n.CreatedAt
        };
    }
}
