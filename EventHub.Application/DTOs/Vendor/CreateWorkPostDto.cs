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

    /// <summary>Optional guest-capacity range (e.g. "50-200 guests" on the listing).</summary>
    public int? MinGuests { get; set; }
    public int? MaxGuests { get; set; }

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

    // Deliberately object-typed presence flags aren't used here — MinGuests/
    // MaxGuests are nullable int?, same as every other optional field on this
    // DTO, so "field omitted" and "field explicitly cleared to null" can't be
    // told apart. That's consistent with how Title/Price/etc. already behave
    // here (an update can't un-set them either) and matches ServiceFormScreen,
    // which always sends both guest fields together on every edit save.
    public int? MinGuests { get; set; }
    public int? MaxGuests { get; set; }
}

public class CreateServicePackageDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Includes { get; set; }
}