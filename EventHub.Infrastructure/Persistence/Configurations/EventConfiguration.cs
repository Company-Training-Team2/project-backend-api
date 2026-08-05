using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(e => e.EventType)
               .IsRequired();

        builder.Property(e => e.TargetDate)
               .IsRequired();

        builder.Property(e => e.GuestCount)
               .IsRequired();

        builder.Property(e => e.TotalBudget)
               .HasPrecision(18, 2);

        builder.Property(e => e.City)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(e => e.Location)
               .IsRequired()
               .HasMaxLength(250);

        builder.Property(e => e.Notes)
               .HasMaxLength(1000);

        // Customer -> Events (1:N)
        builder.HasOne(e => e.Customer)
               .WithMany(c => c.Events)
               .HasForeignKey(e => e.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => e.TargetDate);
        builder.HasIndex(e => e.EventType);
    }
}