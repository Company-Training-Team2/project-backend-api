using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class ChecklistItemConfiguration : IEntityTypeConfiguration<ChecklistItem>
{
    public void Configure(EntityTypeBuilder<ChecklistItem> builder)
    {
        builder.ToTable("ChecklistItems");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(c => c.Description)
               .HasMaxLength(1000);

        builder.Property(c => c.Category)
               .HasMaxLength(100);

        builder.Property(c => c.Priority)
               .IsRequired();

        builder.Property(c => c.IsCompleted)
               .HasDefaultValue(false);

        builder.HasOne(c => c.Event)
               .WithMany(e => e.ChecklistItems)
               .HasForeignKey(c => c.EventId)
               .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(c => c.EventId);
        builder.HasIndex(c => c.Priority);
        builder.HasIndex(c => c.IsCompleted);
    }
}