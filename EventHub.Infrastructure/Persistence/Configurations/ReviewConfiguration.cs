using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rating)
               .IsRequired();

        builder.Property(r => r.Comment)
               .HasMaxLength(1000);

        builder.HasOne(r => r.Booking)
               .WithOne(b => b.Review)
               .HasForeignKey<Review>(r => r.BookingId)
               .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(r => r.BookingId)
               .IsUnique();

        builder.HasIndex(r => r.Rating);

        // Optional validation (SQL CHECK constraint)
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_Review_Rating",
                "[Rating] >= 1 AND [Rating] <= 5");
        });
    }
}