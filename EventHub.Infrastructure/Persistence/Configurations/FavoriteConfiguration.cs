using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Customer)
               .WithMany(x => x.Favorites)
               .HasForeignKey(x => x.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.WorkPost)
               .WithMany(x => x.Favorites)
               .HasForeignKey(x => x.WorkPostId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.CustomerId,
            x.WorkPostId
        }).IsUnique();
    }
}