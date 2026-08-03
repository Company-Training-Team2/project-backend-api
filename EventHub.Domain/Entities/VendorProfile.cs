using EventHub.Domain.Common;
<<<<<<< HEAD
using EventHub.Domain.Enums; // <-- Add this import if not present
=======
using EventHub.Domain.Enums;
>>>>>>> 9c5d494 (feat(auth): complete auth-user-schema (Task 1))

namespace EventHub.Domain.Entities;

public class VendorProfile : SoftDeletableEntity
{
    public int UserId { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string BioDescription { get; set; } = string.Empty;

<<<<<<< HEAD
=======
    public bool IsVerified { get; set; }

>>>>>>> 9c5d494 (feat(auth): complete auth-user-schema (Task 1))
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

    public User User { get; set; } = null!;

    public ICollection<WorkPost> WorkPosts { get; set; } = new List<WorkPost>();
}