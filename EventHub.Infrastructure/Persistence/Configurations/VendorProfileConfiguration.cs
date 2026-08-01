using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class VendorProfileConfiguration : IEntityTypeConfiguration<VendorProfile>
{
    public void Configure(EntityTypeBuilder<VendorProfile> builder)
    {
        // Primary Key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.BusinessName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.BioDescription)
            .HasMaxLength(2000);

        builder.Property(x => x.IsVerified)
            .IsRequired();

        // Convert Enum to String in Database
        builder.Property(x => x.ApprovalStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Relationships & Indexes
        builder.HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<VendorProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Safer than Cascade to protect relational history

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        // Global Query Filter for Soft Delete (Inherited from SoftDeletableEntity)
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}