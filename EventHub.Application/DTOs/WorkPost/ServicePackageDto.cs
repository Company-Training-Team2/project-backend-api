namespace EventHub.Application.DTOs.WorkPost;

public class ServicePackageDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string? Includes { get; set; }
}
