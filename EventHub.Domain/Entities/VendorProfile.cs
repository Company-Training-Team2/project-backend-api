using EventHub.Domain.Common;
using EventHub.Domain.Enums; // <-- Add this import if not present

namespace EventHub.Domain.Entities;

public class VendorProfile : SoftDeletableEntity
{
    public int UserId { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string BioDescription { get; set; } = string.Empty;

    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

    public User User { get; set; } = null!;

    public ICollection<WorkPost> WorkPosts { get; set; } = new List<WorkPost>();
    
}