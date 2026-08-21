using EventHub.Domain.Common;
using EventHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EventHub.Infrastructure.Persistence.Context;

public class ApplicationDbContext
    : IdentityDbContext<User, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<CustomerProfile> CustomerProfiles { get; set; } = null!;
    public DbSet<VendorProfile> VendorProfiles { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<VendorProfileCategory> VendorProfileCategories { get; set; } = null!;
    public DbSet<WorkPost> WorkPosts { get; set; } = null!;
    public DbSet<WorkPostImage> WorkPostImages { get; set; } = null!;
    public DbSet<VendorPortfolioImage> VendorPortfolioImages { get; set; } = null!;
    public DbSet<WorkPostAvailability> WorkPostAvailabilities { get; set; } = null!;
    public DbSet<Favorite> Favorites { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<SavedPaymentMethod> SavedPaymentMethods { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<Guest> Guests { get; set; } = null!;
    public DbSet<ChecklistItem> ChecklistItems { get; set; } = null!;
    public DbSet<Expense> Expenses { get; set; } = null!;
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<ServicePackage> ServicePackages { get; set; } = null!;
    public DbSet<Payout> Payouts { get; set; } = null!;

    // ── Admin module (Module 14) ───────────────────────────────────────────────
    public DbSet<AdminSettings> AdminSettings { get; set; } = null!;
    public DbSet<AdminConversation> AdminConversations { get; set; } = null!;
    public DbSet<AdminConversationMessage> AdminConversationMessages { get; set; } = null!;

    // ── AI Planner (Module 11) ────────────────────────────────────────────────
    public DbSet<AIConversation> AIConversations { get; set; } = null!;
    public DbSet<AIMessage> AIMessages { get; set; } = null!;

    // ── Vendor<->Customer messaging ("Contact Vendor") ─────────────────────────
    public DbSet<Conversation> Conversations { get; set; } = null!;
    public DbSet<ConversationMessage> ConversationMessages { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // Apply Entity Configurations
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);


        // Global Soft Delete Filter
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(SoftDeletableEntity)
                .IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(
                    entityType.ClrType,
                    "e");


                var property = Expression.Property(
                    parameter,
                    nameof(SoftDeletableEntity.IsDeleted));


                var filter = Expression.Lambda(
                    Expression.Equal(
                        property,
                        Expression.Constant(false)),
                    parameter);


                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(filter);
            }
        }


        // User Soft Delete Filter
        modelBuilder.Entity<User>()
            .HasQueryFilter(u => !u.IsDeleted);


        // ── Matching filters for children that have no independent meaning
        // once their (soft-deletable) required parent is gone — resolves EF's
        // "possible unintended filtering" warning (10622) the correct way for
        // these: the child should disappear right along with the parent, same
        // as if it never existed on its own. Booking/Payout/AdminConversation/
        // Conversation are handled differently (their required parent
        // navigation is made optional instead) — those records have real
        // financial/audit significance that must survive the parent being
        // soft-deleted, so silently hiding them here would be the wrong fix,
        // not just a cosmetic one. See each entity's own comment for that path.
        modelBuilder.Entity<AIMessage>().HasQueryFilter(m => !m.AIConversation.IsDeleted);
        modelBuilder.Entity<ChecklistItem>().HasQueryFilter(c => !c.Event.IsDeleted);
        // NOT matching-filtered like the others above: CustomerProfile turned
        // out to be the required end of its own relationships (Booking.
        // Customer, SavedPaymentMethod.Customer) — filtering it here would
        // just push the exact same "unintended filtering" risk one level
        // deeper onto those, including Booking, which already needed the
        // opposite treatment (kept visible on purpose — see Booking.
        // EventId's doc comment). Left as its own deferred fix rather than
        // rushed: User<->CustomerProfile still warns, unchanged from before.
        modelBuilder.Entity<Document>().HasQueryFilter(d => !d.Event.IsDeleted);
        modelBuilder.Entity<Expense>().HasQueryFilter(e => !e.Event.IsDeleted);
        modelBuilder.Entity<Favorite>().HasQueryFilter(f => !f.WorkPost.IsDeleted);
        modelBuilder.Entity<Guest>().HasQueryFilter(g => !g.Event.IsDeleted);
        modelBuilder.Entity<Notification>().HasQueryFilter(n => !n.User.IsDeleted);
        modelBuilder.Entity<ServicePackage>().HasQueryFilter(s => !s.WorkPost.IsDeleted);
        modelBuilder.Entity<VendorProfileCategory>().HasQueryFilter(vc => !vc.Category.IsDeleted);
        modelBuilder.Entity<WorkPostAvailability>().HasQueryFilter(a => !a.WorkPost.IsDeleted);
        modelBuilder.Entity<WorkPostImage>().HasQueryFilter(i => !i.WorkPost.IsDeleted);
        modelBuilder.Entity<VendorPortfolioImage>().HasQueryFilter(i => !i.VendorProfile.IsDeleted);


        // Seed Admin Data
        SeedAdminUser(modelBuilder);
    }


    private static void SeedAdminUser(ModelBuilder modelBuilder)
    {
        var adminRoleId = 3;

        modelBuilder.Entity<IdentityRole<int>>()
            .HasData(
                new IdentityRole<int>
                {
                    Id = adminRoleId,
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                });
    }
}