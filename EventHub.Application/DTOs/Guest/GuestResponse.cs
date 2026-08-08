namespace EventHub.Application.DTOs.Guest;

public class GuestResponse
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string RSVPStatus { get; set; } = string.Empty;
}