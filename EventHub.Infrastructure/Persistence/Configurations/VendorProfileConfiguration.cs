using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class VendorProfileConfiguration : IEntityTypeConfiguration<VendorProfile>
{
    public void Configure(EntityTypeBuilder<VendorProfile> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BusinessName)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.BioDescription)
               .HasMaxLength(2000);

<<<<<<< HEAD
=======
        builder.Property(x => x.IsVerified)
               .IsRequired()
               .HasDefaultValue(false);

>>>>>>> 9c5d494 (feat(auth): complete auth-user-schema (Task 1))
        builder.Property(x => x.ApprovalStatus)
               .IsRequired();

        builder.HasOne(x => x.User)
               .WithOne(x => x.VendorProfile)
               .HasForeignKey<VendorProfile>(x => x.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId)
               .IsUnique();

<<<<<<< HEAD
        // اختياري
        builder.HasIndex(x => x.ApprovalStatus);

       
=======
        builder.HasIndex(x => x.ApprovalStatus);
>>>>>>> 9c5d494 (feat(auth): complete auth-user-schema (Task 1))
    }
}