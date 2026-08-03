using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class WorkPostAvailabilityConfiguration : IEntityTypeConfiguration<WorkPostAvailability>
{
    public void Configure(EntityTypeBuilder<WorkPostAvailability> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Date)
               .IsRequired();

        builder.Property(x => x.IsAvailable)
               .IsRequired();

        builder.Property(x => x.Notes)
               .HasMaxLength(500);

        builder.HasOne(x => x.WorkPost)
               .WithMany(x => x.Availabilities)
               .HasForeignKey(x => x.WorkPostId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.WorkPostId,
            x.Date
        }).IsUnique();
    }
}