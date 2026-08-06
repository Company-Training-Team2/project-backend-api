namespace EventHub.Domain.Enums;

public enum BookingStatus
{
    Pending = 1,
    Confirmed = 2,
    Completed = 3,
    Cancelled = 4,
    Rejected = 5,
    Paid=6
    //Accepted = 2,
    //Paid = 3,
    //Completed = 4,
    //Rejected = 5,
    //Cancelled = 6

    //     Pending = 1,  // Customer submitted, vendor not yet responded
    //Accepted = 2,  // Vendor approved — payment can now be processed
    //Paid = 3,  // Payment confirmed by gateway
    //Completed = 4,  // Service delivered
    //Cancelled = 5,  // Cancelled by customer or vendor
    //Rejected = 6   // Vendor declined the request
}