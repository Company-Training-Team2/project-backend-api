using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType)
               .IsRequired();

        builder.Property(x => x.TargetDate)
               .IsRequired();

        builder.Property(x => x.GuestCount)
               .IsRequired();

        builder.Property(x => x.TotalBudget)
               .HasPrecision(18, 2);

        builder.Property(x => x.Notes)
               .HasMaxLength(1000);

        builder.HasOne(x => x.Customer)
               .WithMany(x => x.Events)
               .HasForeignKey(x => x.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CustomerId);
    }
}
