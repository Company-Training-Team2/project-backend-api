using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName)
               .IsRequired()
               .HasMaxLength(150);

<<<<<<< HEAD
=======
        builder.Property(x => x.PhoneNumber)
               .HasMaxLength(20);

>>>>>>> 9c5d494 (feat(auth): complete auth-user-schema (Task 1))
        builder.Property(x => x.City)
               .HasMaxLength(100);

        builder.HasOne(x => x.User)
               .WithOne(x => x.CustomerProfile)
               .HasForeignKey<CustomerProfile>(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId)
               .IsUnique();
    }
}