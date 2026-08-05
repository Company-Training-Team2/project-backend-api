namespace EventHub.Application.DTOs.WorkPost;

public class WorkPostImageDto
{
    public int Id { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }
}
