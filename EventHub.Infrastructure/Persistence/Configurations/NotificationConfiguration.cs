using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Type)
               .IsRequired();

        builder.Property(n => n.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(n => n.Body)
               .IsRequired()
               .HasMaxLength(1000);

        builder.Property(n => n.IsRead)
               .HasDefaultValue(false);

        // .WithMany() with no argument told EF "Notification.User has no
        // matching collection on User" — but User.Notifications *is* that
        // collection, just never paired here. EF's convention discovery then
        // treated User.Notifications as a second, separate relationship it
        // had to invent its own FK for, since UserId was already claimed by
        // this one: hence the phantom Notifications.UserId1 shadow
        // column/FK/index that's existed since the very first migration
        // (see EF Core warning EF1002/model validation 10625). Naming the
        // real inverse collapses it back into the single relationship it was
        // always meant to be.
        builder.HasOne(n => n.User)
               .WithMany(u => u.Notifications)
               .HasForeignKey(n => n.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.IsRead);
        builder.HasIndex(n => n.Type);
        builder.HasIndex(n => n.CreatedAt);
    }
}