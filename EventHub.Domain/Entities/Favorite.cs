using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

public class Favorite : BaseEntity
{


    public int CustomerId { get; set; }

    public int WorkPostId { get; set; }

    public CustomerProfile Customer { get; set; } = null!;

    public WorkPost WorkPost { get; set; } = null!;
}