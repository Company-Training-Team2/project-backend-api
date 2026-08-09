using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class AIMessageConfiguration : IEntityTypeConfiguration<AIMessage>
{
    public void Configure(EntityTypeBuilder<AIMessage> builder)
    {
        builder.ToTable("AIMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Content)
               .IsRequired()
               .HasMaxLength(4000);

        builder.Property(x => x.Sender)
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.HasOne(x => x.AIConversation)
               .WithMany(x => x.Messages)
               .HasForeignKey(x => x.AIConversationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}