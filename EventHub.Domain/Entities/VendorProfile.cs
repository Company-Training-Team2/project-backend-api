using EventHub.Domain.Common;
using EventHub.Domain.Enums;
namespace EventHub.Domain.Entities;

public class VendorProfile : SoftDeletableEntity
{
    public int UserId { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string BioDescription { get; set; } = string.Empty;

    public bool IsVerified { get; set; }

    public ApprovalStatus ApprovalStatus { get; set; }

    public User User { get; set; } = null!;
}
