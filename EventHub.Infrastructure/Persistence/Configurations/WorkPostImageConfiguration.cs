using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class WorkPostImageConfiguration : IEntityTypeConfiguration<WorkPostImage>
{
    public void Configure(EntityTypeBuilder<WorkPostImage> builder)
    {
        builder.ToTable("WorkPostImages");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ImageUrl)
               .IsRequired()
               .HasMaxLength(1000);

        builder.Property(i => i.IsPrimary)
               .HasDefaultValue(false);

        builder.Property(i => i.UploadedAt)
               .IsRequired();

        // WorkPost (1) -> (Many) Images
        builder.HasOne(i => i.WorkPost)
               .WithMany(w => w.Images)
               .HasForeignKey(i => i.WorkPostId)
               .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(i => i.WorkPostId);

        builder.HasIndex(i => i.IsPrimary);

        builder.HasIndex(i => i.UploadedAt);
    }
}