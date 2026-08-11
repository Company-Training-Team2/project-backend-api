using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(x => x.Description)
               .HasMaxLength(1000);

        builder.HasIndex(x => x.Name)
               .IsUnique();

        builder.HasIndex(x => x.IsDeleted);

        // Seed data — the real Categories table had no rows, forcing the
        // frontend to fall back to a mock fixture (lib/mock/categories.ts)
        // for every category dropdown/filter. Same category set as that
        // fixture, now backed by real rows with real Ids so WorkPost.CategoryId
        // can reference them for real.
        var seededAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new Category { Id = 1, Name = "Catering", Description = "Fine dining, buffets, tastings", CreatedAt = seededAt, IsDeleted = false },
            new Category { Id = 2, Name = "Florals", Description = "Bouquets, centerpieces, installations", CreatedAt = seededAt, IsDeleted = false },
            new Category { Id = 3, Name = "Venue", Description = "Halls, estates, rooftops, gardens", CreatedAt = seededAt, IsDeleted = false },
            new Category { Id = 4, Name = "Photography", Description = "Photo & video coverage", CreatedAt = seededAt, IsDeleted = false },
            new Category { Id = 5, Name = "Makeup Artist", Description = "Bridal & event makeup", CreatedAt = seededAt, IsDeleted = false },
            new Category { Id = 6, Name = "Decoration", Description = "Styling, tablescapes, lighting", CreatedAt = seededAt, IsDeleted = false },
            new Category { Id = 7, Name = "Music", Description = "Live bands, DJs, ensembles", CreatedAt = seededAt, IsDeleted = false },
            new Category { Id = 8, Name = "Planning", Description = "Full-service event coordination", CreatedAt = seededAt, IsDeleted = false }
        );
    }
}