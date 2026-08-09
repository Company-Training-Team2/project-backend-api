using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class SavedPaymentMethodConfiguration : IEntityTypeConfiguration<SavedPaymentMethod>
{
    public void Configure(EntityTypeBuilder<SavedPaymentMethod> builder)
    {
        builder.ToTable("SavedPaymentMethods");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type)
               .IsRequired();

        builder.Property(m => m.MaskedNumber)
               .IsRequired()
               .HasMaxLength(32);

        builder.Property(m => m.CardHolderName)
               .HasMaxLength(200);

        builder.Property(m => m.GatewayToken)
               .HasMaxLength(200);

        builder.Property(m => m.CreatedAt)
               .IsRequired();

        // CustomerProfile (1) -> (Many) SavedPaymentMethods
        builder.HasOne(m => m.Customer)
               .WithMany(c => c.SavedPaymentMethods)
               .HasForeignKey(m => m.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.CustomerId);
    }
}
