using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("Favorites");

        builder.HasKey(f => f.Id);

        builder.HasOne(f => f.Customer)
               .WithMany(c => c.Favorites)
               .HasForeignKey(f => f.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.WorkPost)
               .WithMany(w => w.Favorites)
               .HasForeignKey(f => f.WorkPostId)
               .OnDelete(DeleteBehavior.Cascade);

        // Prevent duplicate favorites
        builder.HasIndex(f => new
        {
            f.CustomerId,
            f.WorkPostId
        }).IsUnique();

        // Indexes
        builder.HasIndex(f => f.CustomerId);
        builder.HasIndex(f => f.WorkPostId);
    }
}