using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(x => x.Role)
               .IsRequired();


        builder.Property(x => x.IsEmailVerified)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(x => x.IsActive)
               .IsRequired()
               .HasDefaultValue(true);

        builder.Property(x => x.IsMfaEnabled)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(x => x.EmailVerificationToken)
               .HasMaxLength(500);

        builder.Property(x => x.RefreshToken)
               .HasMaxLength(500);

        builder.Property(x => x.MfaSecret)
               .HasMaxLength(500);

        // Indexes for auth lookups
        builder.HasIndex(x => x.Email)
               .IsUnique();

        builder.HasIndex(x => x.RefreshToken)
               .HasFilter("[RefreshToken] IS NOT NULL");

        builder.HasIndex(x => x.Role);

        builder.HasIndex(x => x.EmailVerificationToken)
               .HasFilter("[EmailVerificationToken] IS NOT NULL");
    }
}