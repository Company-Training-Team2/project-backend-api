using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

public class VendorProfile : SoftDeletableEntity
{
    public int UserId { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string BioDescription { get; set; } = string.Empty;

    public bool IsVerified { get; set; }

    public string ApprovalStatus { get; set; } = string.Empty;

    public User User { get; set; } = null!;

    public ICollection<WorkPost> WorkPosts { get; set; } = new List<WorkPost>();
}