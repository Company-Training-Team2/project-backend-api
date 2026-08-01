using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class WorkPostConfiguration : IEntityTypeConfiguration<WorkPost>
{
    public void Configure(EntityTypeBuilder<WorkPost> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.Description)
               .IsRequired()
               .HasMaxLength(5000);

        builder.Property(x => x.Price)
               .HasPrecision(18, 2);

        builder.Property(x => x.City)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.Address)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(x => x.ApprovalStatus)
               .IsRequired();

        builder.HasOne(x => x.VendorProfile)
               .WithMany(x => x.WorkPosts)
               .HasForeignKey(x => x.VendorProfileId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Category)
               .WithMany(x => x.WorkPosts)
               .HasForeignKey(x => x.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReviewedByAdmin)
               .WithMany()
               .HasForeignKey(x => x.ReviewedByAdminId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.VendorProfileId);

        builder.HasIndex(x => x.CategoryId);

        builder.HasIndex(x => x.City);

        builder.HasIndex(x => x.ApprovalStatus);
    }
}