namespace EventHub.Application.DTOs.Guest;

public class CreateGuestRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}