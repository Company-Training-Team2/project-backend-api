using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
               .HasPrecision(18, 2)
               .IsRequired();

        builder.Property(p => p.PaymentMethod)
               .IsRequired();

        builder.Property(p => p.PaymentStatus)
               .IsRequired();

        builder.Property(p => p.TransactionId)
               .HasMaxLength(100);

        builder.Property(p => p.PaymentGateway)
               .HasMaxLength(100);

        builder.Property(p => p.PaidAt);

        builder.HasOne(p => p.Booking)
               .WithOne(b => b.Payment)
               .HasForeignKey<Payment>(p => p.BookingId)
               .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.BookingId)
               .IsUnique();

        builder.HasIndex(p => p.PaymentStatus);

        builder.HasIndex(p => p.TransactionId)
               .IsUnique()
               .HasFilter("[TransactionId] IS NOT NULL");
    }
}