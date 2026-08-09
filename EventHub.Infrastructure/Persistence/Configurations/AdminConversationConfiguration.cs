using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class AdminConversationConfiguration : IEntityTypeConfiguration<AdminConversation>
{
    public void Configure(EntityTypeBuilder<AdminConversation> builder)
    {
        builder.ToTable("AdminConversations");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Subject)
               .HasMaxLength(300)
               .IsRequired();

        builder.Property(c => c.Status)
               .HasMaxLength(20)
               .IsRequired();

        builder.HasOne(c => c.User)
               .WithMany()
               .HasForeignKey(c => c.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Messages)
               .WithOne(m => m.Conversation)
               .HasForeignKey(m => m.ConversationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AdminConversationMessageConfiguration
    : IEntityTypeConfiguration<AdminConversationMessage>
{
    public void Configure(EntityTypeBuilder<AdminConversationMessage> builder)
    {
        builder.ToTable("AdminConversationMessages");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Body)
               .HasMaxLength(4000)
               .IsRequired();
    }
}
