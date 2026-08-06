namespace EventHub.Application.DTOs;

/// <summary>
/// Ids are int, matching BaseEntity (no GUIDs anywhere in this domain).
/// CustomerId is CustomerProfile.Id (not the ASP.NET Identity User.Id).
/// </summary>
public class EventResponse
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public int GuestCount { get; set; }
    public decimal TotalBudget { get; set; }
    public string City { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
