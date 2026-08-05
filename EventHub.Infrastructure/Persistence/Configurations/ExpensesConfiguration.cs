using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Category)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(e => e.Description)
               .IsRequired()
               .HasMaxLength(1000);

        builder.Property(e => e.Amount)
               .HasPrecision(18, 2);

        builder.Property(e => e.Status)
               .IsRequired();

        builder.Property(e => e.Date)
               .IsRequired();

        // Event (1) -> (Many) Expenses
        builder.HasOne(e => e.Event)
               .WithMany(ev => ev.Expenses)
               .HasForeignKey(e => e.EventId)
               .OnDelete(DeleteBehavior.Cascade);

        // Booking (1) -> (0..1) Expense
        builder.HasOne(e => e.Booking)
               .WithOne(b => b.Expense)
               .HasForeignKey<Expense>(e => e.BookingId)
               .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(e => e.EventId);
        builder.HasIndex(e => e.BookingId)
               .IsUnique();
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.Date);
    }
}