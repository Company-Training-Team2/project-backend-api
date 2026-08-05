using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class WorkPostAvailabilityConfiguration : IEntityTypeConfiguration<WorkPostAvailability>
{
    public void Configure(EntityTypeBuilder<WorkPostAvailability> builder)
    {
        builder.ToTable("WorkPostAvailabilities");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Date)
               .IsRequired();

        builder.Property(a => a.IsAvailable)
               .HasDefaultValue(true);

        builder.Property(a => a.Notes)
               .HasMaxLength(1000);

        // WorkPost (1) -> (Many) Availabilities
        builder.HasOne(a => a.WorkPost)
               .WithMany(w => w.Availabilities)
               .HasForeignKey(a => a.WorkPostId)
               .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(a => a.WorkPostId);

        builder.HasIndex(a => a.Date);

        builder.HasIndex(a => a.IsAvailable);

        // Prevent duplicate availability records for the same date
        builder.HasIndex(a => new
        {
            a.WorkPostId,
            a.Date
        }).IsUnique();
    }
}