using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class AIConversationConfiguration : IEntityTypeConfiguration<AIConversation>
{
    public void Configure(EntityTypeBuilder<AIConversation> builder)
    {
        builder.ToTable("AIConversations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.HasOne(x => x.Event)
               .WithMany()
               .HasForeignKey(x => x.EventId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Messages)
               .WithOne(x => x.AIConversation)
               .HasForeignKey(x => x.AIConversationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}