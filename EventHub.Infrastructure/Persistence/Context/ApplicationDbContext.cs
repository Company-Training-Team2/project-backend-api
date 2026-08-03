using EventHub.Domain.Common;
using EventHub.Domain.Entities;
<<<<<<< HEAD
=======
using EventHub.Domain.Enums;
>>>>>>> 9c5d494 (feat(auth): complete auth-user-schema (Task 1))
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    public DbSet<WorkPost> WorkPosts { get; set; } = null!;
    public DbSet<WorkPostImage> WorkPostImages { get; set; } = null!;
    public DbSet<Favorite> Favorites { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);

        // Global Soft Delete Filter
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(SoftDeletableEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");

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
<<<<<<< HEAD
=======

        // Seed default Admin account (PRD: Admin accounts are never publicly registered)
        SeedAdminUser(modelBuilder);
    }

    private static void SeedAdminUser(ModelBuilder modelBuilder)
    {
        // Default admin credentials - CHANGE THESE IN PRODUCTION!
        const string adminEmail = "admin@eventhub.com";
        const string adminPassword = "Admin@123456";

        var hasher = new PasswordHasher<User>();

        var adminUser = new User
        {
            Id = 1,
            UserName = adminEmail,
            NormalizedUserName = adminEmail.ToUpper(),
            Email = adminEmail,
            NormalizedEmail = adminEmail.ToUpper(),
            EmailConfirmed = true,
            IsEmailVerified = true,
            Role = UserRole.Admin,
            IsActive = true,
            // ⚠️ STATIC VALUES - لا تستخدم Guid.NewGuid() هنا!
            SecurityStamp = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
            ConcurrencyStamp = "b2c3d4e5-f6a7-8901-bcde-f12345678901"
        };

        adminUser.PasswordHash = hasher.HashPassword(adminUser, adminPassword);

        modelBuilder.Entity<User>().HasData(adminUser);

        // Seed 3 Roles - STATIC ConcurrencyStamp
        modelBuilder.Entity<IdentityRole<int>>().HasData(
            new IdentityRole<int> 
            { 
                Id = 1, 
                Name = "Customer", 
                NormalizedName = "CUSTOMER", 
                ConcurrencyStamp = "c3d4e5f6-a7b8-9012-cdef-123456789012" 
            },
            new IdentityRole<int> 
            { 
                Id = 2, 
                Name = "Vendor", 
                NormalizedName = "VENDOR", 
                ConcurrencyStamp = "d4e5f6a7-b8c9-0123-defa-123456789013" 
            },
            new IdentityRole<int> 
            { 
                Id = 3, 
                Name = "Admin", 
                NormalizedName = "ADMIN", 
                ConcurrencyStamp = "e5f6a7b8-c9d0-1234-efab-123456789014" 
            }
        );
>>>>>>> 9c5d494 (feat(auth): complete auth-user-schema (Task 1))
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<SoftDeletableEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
            }
        }

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}