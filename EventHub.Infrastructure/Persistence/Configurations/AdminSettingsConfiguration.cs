using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class AdminSettingsConfiguration : IEntityTypeConfiguration<AdminSettings>
{
    public void Configure(EntityTypeBuilder<AdminSettings> builder)
    {
        builder.ToTable("AdminSettings");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.CommissionPercentage)
               .HasColumnType("decimal(5,2)");

        builder.Property(s => s.TaxPercentage)
               .HasColumnType("decimal(5,2)");

        builder.Property(s => s.PlatformName)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(s => s.PlatformLogoUrl)
               .HasMaxLength(500);

        builder.Property(s => s.SupportEmail)
               .HasMaxLength(200);
    }
}
