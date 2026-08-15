using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class VendorProfileCategoryConfiguration : IEntityTypeConfiguration<VendorProfileCategory>
{
    public void Configure(EntityTypeBuilder<VendorProfileCategory> builder)
    {
        builder.ToTable("VendorProfileCategories");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.VendorProfile)
               .WithMany(v => v.VendorCategories)
               .HasForeignKey(x => x.VendorProfileId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Category)
               .WithMany()
               .HasForeignKey(x => x.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        // A vendor can't select the same category twice.
        builder.HasIndex(x => new { x.VendorProfileId, x.CategoryId })
               .IsUnique();
    }
}
