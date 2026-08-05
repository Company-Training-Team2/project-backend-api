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
    public DbSet<WorkPost> WorkPosts { get; set; } = null!;
    public DbSet<WorkPostImage> WorkPostImages { get; set; } = null!;
    public DbSet<WorkPostAvailability> WorkPostAvailabilities { get; set; } = null!;
    public DbSet<Favorite> Favorites { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<Guest> Guests { get; set; } = null!;
    public DbSet<ChecklistItem> ChecklistItems { get; set; } = null!;
    public DbSet<Expense> Expenses { get; set; } = null!;
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<ServicePackage> ServicePackages { get; set; } = null!;


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