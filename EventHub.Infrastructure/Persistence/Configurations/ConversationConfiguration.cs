using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");
        builder.HasKey(c => c.Id);

        builder.HasOne(c => c.CustomerUser)
               .WithMany()
               .HasForeignKey(c => c.CustomerUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.VendorUser)
               .WithMany()
               .HasForeignKey(c => c.VendorUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.WorkPost)
               .WithMany()
               .HasForeignKey(c => c.WorkPostId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Messages)
               .WithOne(m => m.Conversation)
               .HasForeignKey(m => m.ConversationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessage>
{
    public void Configure(EntityTypeBuilder<ConversationMessage> builder)
    {
        builder.ToTable("ConversationMessages");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Body)
               .HasMaxLength(4000)
               .IsRequired();
    }
}
