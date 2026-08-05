using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> builder)
    {
        builder.ToTable("CustomerProfiles");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FullName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(c => c.PhoneNumber)
               .HasMaxLength(20);

        builder.Property(c => c.City)
               .HasMaxLength(100);

        builder.Property(c => c.AvatarUrl)
               .HasMaxLength(500);

        // One User -> One CustomerProfile
        builder.HasOne(c => c.User)
               .WithOne(u => u.CustomerProfile)
               .HasForeignKey<CustomerProfile>(c => c.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => c.UserId)
               .IsUnique();

        builder.HasIndex(c => c.FullName);
    }
}