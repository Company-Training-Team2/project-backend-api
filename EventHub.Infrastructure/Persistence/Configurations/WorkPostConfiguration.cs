using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class WorkPostConfiguration : IEntityTypeConfiguration<WorkPost>
{
    public void Configure(EntityTypeBuilder<WorkPost> builder)
    {
        builder.ToTable("WorkPosts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.Description)
               .IsRequired()
               .HasMaxLength(3000);

        builder.Property(x => x.Price)
               .HasPrecision(18, 2);

        builder.Property(x => x.City)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.Address)
               .IsRequired()
               .HasMaxLength(300);

        builder.Property(x => x.ApprovalStatus)
               .IsRequired();

        // VendorProfile (1) -> (Many) WorkPosts
        builder.HasOne(x => x.VendorProfile)
               .WithMany(v => v.WorkPosts)
               .HasForeignKey(x => x.VendorProfileId)
               .OnDelete(DeleteBehavior.Restrict);

        // Category (1) -> (Many) WorkPosts
        builder.HasOne(x => x.Category)
               .WithMany(c => c.WorkPosts)
               .HasForeignKey(x => x.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        // Admin reviewer (optional)
        builder.HasOne(x => x.ReviewedByAdmin)
               .WithMany()
               .HasForeignKey(x => x.ReviewedByAdminId)
               .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => x.VendorProfileId);

        builder.HasIndex(x => x.CategoryId);

        builder.HasIndex(x => x.ReviewedByAdminId);

        builder.HasIndex(x => x.ApprovalStatus);

        builder.HasIndex(x => x.City);

        builder.HasIndex(x => x.IsDeleted);

        builder.HasIndex(x => new
        {
            x.CategoryId,
            x.City
        });

        builder.HasIndex(x => new
        {
            x.VendorProfileId,
            x.ApprovalStatus
        });
    }
}