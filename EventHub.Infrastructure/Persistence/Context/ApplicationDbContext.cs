using EventHub.Domain.Common;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
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
    public DbSet<Favorite> Favorites { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<WorkPostAvailability> WorkPostAvailabilities { get; set; } = null!;

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

        // Global filter for User entity
        modelBuilder.Entity<User>()
            .HasQueryFilter(u => !u.IsDeleted);

        // Seed default Admin account
        SeedAdminUser(modelBuilder);
    }

    private static void SeedAdminUser(ModelBuilder modelBuilder)
    {
        // keep the whole method from authentication branch
        // (your existing SeedAdminUser code goes here)
    }