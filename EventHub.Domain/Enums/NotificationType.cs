namespace EventHub.Domain.Enums;

/// <summary>Per audit Module 10 notification types.</summary>
public enum NotificationType
{
    BookingStatusUpdate = 1,
    VendorMatch = 2,
    SecurityAlert = 3,
    PaymentReceipt = 4,
    NewReview = 5,
    Message = 6,
    SystemGeneral = 7
}