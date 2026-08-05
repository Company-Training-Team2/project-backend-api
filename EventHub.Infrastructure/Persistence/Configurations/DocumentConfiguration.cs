using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Type)
               .IsRequired();

        builder.Property(d => d.FileName)
               .IsRequired()
               .HasMaxLength(255);

        builder.Property(d => d.FileUrl)
               .HasMaxLength(1000);

        builder.Property(d => d.Amount)
               .HasPrecision(18, 2);

        builder.Property(d => d.Status)
               .HasMaxLength(50);

        builder.Property(d => d.UploadedAt)
               .IsRequired();

        builder.HasOne(d => d.Event)
               .WithMany(e => e.Documents)
               .HasForeignKey(d => d.EventId)
               .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(d => d.EventId);
        builder.HasIndex(d => d.Type);
        builder.HasIndex(d => d.UploadedAt);
    }
}