using EventHub.Application.Interfaces;
using EventHub.Application.Services;
using EventHub.Domain.Entities;
using EventHub.Infrastructure.Persistence.Context;
using EventHub.Infrastructure.Persistence.Repositories;
using EventHub.Infrastructure.Persistence.UnitOfWork;
using EventHub.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =========================================
// Database
// =========================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("EventHub.Infrastructure"));

    options.ConfigureWarnings(warnings =>
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// =========================================
// Identity
// =========================================
builder.Services.AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// =========================================
// Repositories & Unit Of Work
// =========================================
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


// =========================================
// User Management Services
// =========================================
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();


// =========================================
// Booking Management Services
// =========================================
builder.Services.AddScoped<IWorkPostAvailabilityService, WorkPostAvailabilityService>();


// =========================================
// AutoMapper
// =========================================
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


// =========================================
// API
// =========================================
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "EventHub API",
        Version = "v1"
    });
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();


var app = builder.Build();


// =========================================
// Middleware
// =========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();