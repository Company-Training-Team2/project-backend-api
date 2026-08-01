using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class WorkPostImageConfiguration : IEntityTypeConfiguration<WorkPostImage>
{
    public void Configure(EntityTypeBuilder<WorkPostImage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ImageUrl)
               .IsRequired()
               .HasMaxLength(1000);

        builder.Property(x => x.UploadedAt)
               .IsRequired();

        builder.HasOne(x => x.WorkPost)
               .WithMany(x => x.Images)
               .HasForeignKey(x => x.WorkPostId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.WorkPostId);
    }
}