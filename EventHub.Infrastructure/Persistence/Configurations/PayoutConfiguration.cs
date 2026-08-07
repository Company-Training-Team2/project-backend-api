using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class PayoutConfiguration : IEntityTypeConfiguration<Payout>
{
    public void Configure(EntityTypeBuilder<Payout> builder)
    {
        builder.ToTable("Payouts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
               .HasPrecision(18, 2)
               .IsRequired();

        builder.Property(x => x.Status)
               .IsRequired();

        builder.Property(x => x.ProcessedAt);

        builder.HasOne(x => x.VendorProfile)
               .WithMany(v => v.Payouts)
               .HasForeignKey(x => x.VendorProfileId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Payment)
               .WithOne(p => p.Payout)
               .HasForeignKey<Payout>(x => x.PaymentId)
               .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.VendorProfileId);

        builder.HasIndex(x => x.PaymentId)
               .IsUnique();

        builder.HasIndex(x => x.Status);
    }
}
