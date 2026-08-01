using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

public class Category : SoftDeletableEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; } = string.Empty;

    public ICollection<WorkPost> WorkPosts { get; set; } = new List<WorkPost>();
}
