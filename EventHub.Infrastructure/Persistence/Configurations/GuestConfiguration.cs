using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class GuestConfiguration : IEntityTypeConfiguration<Guest>
{
    public void Configure(EntityTypeBuilder<Guest> builder)
    {
        builder.ToTable("Guests");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(g => g.Email)
               .HasMaxLength(255);

        builder.Property(g => g.PhoneNumber)
               .HasMaxLength(20);

        builder.Property(g => g.RSVPStatus)
               .IsRequired();

        // Event (1) -> (Many) Guests
        builder.HasOne(g => g.Event)
               .WithMany(e => e.Guests)
               .HasForeignKey(g => g.EventId)
               .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(g => g.EventId);
        builder.HasIndex(g => g.RSVPStatus);
        builder.HasIndex(g => g.Email);
    }
}