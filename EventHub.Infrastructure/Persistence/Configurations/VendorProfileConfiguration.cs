using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class VendorProfileConfiguration : IEntityTypeConfiguration<VendorProfile>
{
    public void Configure(EntityTypeBuilder<VendorProfile> builder)
    {
        builder.ToTable("VendorProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BusinessName)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.BioDescription)
               .HasMaxLength(2000);

        builder.Property(x => x.PhoneNumber)
               .HasMaxLength(20);

        builder.Property(x => x.City)
               .HasMaxLength(100);

        builder.Property(x => x.LogoUrl)
               .HasMaxLength(500);

        builder.Property(x => x.CoverImageUrl)
               .HasMaxLength(500);

        builder.Property(x => x.CommercialRegistrationPath)
               .HasMaxLength(500);

        builder.Property(x => x.NationalIdPath)
               .HasMaxLength(500);

        builder.Property(x => x.BusinessLicensePath)
               .HasMaxLength(500);

        builder.Property(x => x.IsVerified)
               .HasDefaultValue(false);

        builder.Property(x => x.ApprovalStatus)
               .IsRequired();

        // ─── Bank account (Payment module) ────────────────────────────────────
        builder.Property(x => x.BankName)
               .HasMaxLength(200);

        builder.Property(x => x.AccountName)
               .HasMaxLength(200);

        builder.Property(x => x.AccountNumber)
               .HasMaxLength(50);

        builder.HasOne(x => x.User)
               .WithOne(x => x.VendorProfile)
               .HasForeignKey<VendorProfile>(x => x.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId)
               .IsUnique();

        builder.HasIndex(x => x.BusinessName);

        builder.HasIndex(x => x.ApprovalStatus);

        builder.HasIndex(x => x.IsVerified);

        builder.HasIndex(x => x.IsDeleted);
    }
}
