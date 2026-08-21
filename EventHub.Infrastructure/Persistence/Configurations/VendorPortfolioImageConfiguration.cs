using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class VendorPortfolioImageConfiguration : IEntityTypeConfiguration<VendorPortfolioImage>
{
    public void Configure(EntityTypeBuilder<VendorPortfolioImage> builder)
    {
        builder.ToTable("VendorPortfolioImages");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ImageUrl)
               .IsRequired()
               .HasMaxLength(1000);

        // VendorProfile (1) -> (Many) PortfolioImages
        builder.HasOne(i => i.VendorProfile)
               .WithMany(v => v.PortfolioImages)
               .HasForeignKey(i => i.VendorProfileId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.VendorProfileId);
    }
}
