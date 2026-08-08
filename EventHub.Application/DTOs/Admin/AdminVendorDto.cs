namespace EventHub.Application.DTOs.Admin;

/// <summary>Vendor record as seen by Admin — used in approval queue and directory.</summary>
public class AdminVendorDto
{
    public int VendorProfileId { get; set; }

    public int UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string BusinessName { get; set; } = string.Empty;

    public string BioDescription { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? City { get; set; }

    public string ApprovalStatus { get; set; } = string.Empty;

    public bool IsVerified { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }
}