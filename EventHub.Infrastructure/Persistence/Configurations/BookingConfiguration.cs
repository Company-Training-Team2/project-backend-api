using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
               .IsRequired();

        builder.Property(x => x.TotalPrice)
               .HasPrecision(18, 2);

        builder.Property(x => x.Quantity)
               .IsRequired();

        builder.Property(x => x.Notes)
               .HasMaxLength(1000);

        // Customer FK
        builder.HasOne(x => x.Customer)
               .WithMany(x => x.Bookings)
               .HasForeignKey(x => x.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Event)
               .WithMany(x => x.Bookings)
               .HasForeignKey(x => x.EventId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.WorkPost)
               .WithMany(x => x.Bookings)
               .HasForeignKey(x => x.WorkPostId)
               .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.EventId);
        builder.HasIndex(x => x.WorkPostId);
        builder.HasIndex(x => x.Status);
    }
}