namespace EventHub.Application.DTOs.Vendor;

public class CreateWorkPostDto
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Base / fallback price. Tiered pricing is managed via ServicePackages.
    /// </summary>
    public decimal Price { get; set; }
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    /// <summary>Optional service packages (tiered pricing) to create along with the post.</summary>
    public List<CreateServicePackageDto> ServicePackages { get; set; } = new();
}

public class UpdateWorkPostDto
{
    public int? CategoryId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
}

public class CreateServicePackageDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Includes { get; set; }
}