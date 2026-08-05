using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.Property(u => u.Role)
               .IsRequired();

        builder.Property(u => u.IsActive)
               .HasDefaultValue(true);

        builder.Property(u => u.IsDeleted)
               .HasDefaultValue(false);

        builder.Property(u => u.IsEmailVerified)
               .HasDefaultValue(false);

        builder.Property(u => u.IsMfaEnabled)
               .HasDefaultValue(false);

        builder.Property(u => u.EmailVerificationCode)
               .HasMaxLength(6);

        builder.Property(u => u.PasswordResetCode)
               .HasMaxLength(6);

        builder.Property(u => u.RefreshToken)
               .HasMaxLength(500);

        builder.Property(u => u.MfaSecret)
               .HasMaxLength(200);

        builder.Property(u => u.CreatedAt)
               .IsRequired();

        // Indexes
        builder.HasIndex(u => u.Role);

        builder.HasIndex(u => u.IsActive);

        builder.HasIndex(u => u.IsDeleted);

        builder.HasIndex(u => u.IsEmailVerified);

        builder.HasIndex(u => u.EmailVerificationCode);

        builder.HasIndex(u => u.PasswordResetCode);

        builder.HasIndex(u => u.RefreshToken);
    }
}