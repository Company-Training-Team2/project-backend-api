using EventHub.Application.Interfaces;
using EventHub.Application.Services;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using EventHub.Infrastructure.Persistence.Context;
using EventHub.Infrastructure.Persistence.Repositories;
using EventHub.Infrastructure.Persistence.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =========================================
// Database
// =========================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("EventHub.Infrastructure")));

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
// Services
// =========================================
builder.Services.AddScoped<IWorkPostAvailabilityService, WorkPostAvailabilityService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// =========================================
// AutoMapper
// =========================================
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// =========================================
// Controllers
// =========================================
builder.Services.AddControllers();

// =========================================
// Swagger
// =========================================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "EventHub API",
        Version = "v1"
    });
});

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