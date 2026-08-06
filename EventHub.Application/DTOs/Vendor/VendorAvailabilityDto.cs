namespace EventHub.Application.DTOs.Vendor;

/// <summary>
/// GET /PUT /api/vendor/availability
/// </summary>
public class VendorAvailabilityDto
{
    public int WorkPostId { get; set; }
    public string WorkPostTitle { get; set; } = string.Empty;
    public List<AvailabilityDayDto> Days { get; set; } = new();
}

public class AvailabilityDayDto
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public bool IsAvailable { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAvailabilityDto
{
    /// <summary>workPostId -> list of day updates to apply.</summary>
    public int WorkPostId { get; set; }
    public List<AvailabilityDayUpdateDto> Days { get; set; } = new();
}

public class AvailabilityDayUpdateDto
{
    public DateOnly Date { get; set; }
    public bool IsAvailable { get; set; }
    public string? Notes { get; set; }
}