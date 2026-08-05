using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class ServicePackageConfiguration : IEntityTypeConfiguration<ServicePackage>
{
    public void Configure(EntityTypeBuilder<ServicePackage> builder)
    {
        builder.ToTable("ServicePackages");

        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(sp => sp.Description)
               .HasMaxLength(1000);

        builder.Property(sp => sp.Price)
               .HasPrecision(18, 2)
               .IsRequired();

        builder.Property(sp => sp.Includes)
               .HasMaxLength(2000);

        builder.Property(sp => sp.IsActive)
               .HasDefaultValue(true);

        // WorkPost (1) -> (Many) ServicePackages
        builder.HasOne(sp => sp.WorkPost)
               .WithMany(w => w.ServicePackages)
               .HasForeignKey(sp => sp.WorkPostId)
               .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(sp => sp.WorkPostId);

        builder.HasIndex(sp => sp.IsActive);

        builder.HasIndex(sp => new
        {
            sp.WorkPostId,
            sp.Name
        }).IsUnique();
    }
}