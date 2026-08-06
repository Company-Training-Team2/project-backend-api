using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using EventHub.API.Hubs;
using EventHub.API.RealTime;
using EventHub.Application.Interfaces;
using EventHub.Application.Services;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using EventHub.Infrastructure.Persistence.Context;
//using EventHub.Infrastructure.Persistence.Repositories;
using EventHub.Infrastructure.Persistence.Repositories;
using EventHub.Infrastructure.Persistence.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// =========================================
// Database
// =========================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("EventHub.Infrastructure"));

    options.ConfigureWarnings(w =>
        w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// =========================================
// Identity
// =========================================
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// =========================================
// JWT Authentication
// =========================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwt = builder.Configuration.GetSection("Jwt");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt["Secret"]!)),
        ClockSkew = TimeSpan.Zero
    };

    // SignalR JS clients can't set an Authorization header on the WebSocket
    // handshake, so the token is sent as ?access_token=... instead — read it
    // there for requests hitting our notification hub (audit Module 10).
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// =========================================
// CORS (audit Module 1: enable global CORS)
// =========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // TODO: tighten to specific origin(s) before production
        policy
            .WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:3000" })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
// =========================================
// Event Module
// =========================================
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IGuestRepository, GuestRepository>();

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IGuestService, GuestService>();
// =========================================
// Repositories & Unit Of Work
// =========================================
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
// =========================================
// Application Services
// =========================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IMfaService, MfaService>();
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
// User module (audit Module 12)
builder.Services.AddScoped<IUserService, UserService>();

// Admin module (replaces thin AdminUserService — audit Module 14)
builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IWorkPostAvailabilityService, WorkPostAvailabilityService>();

// Home & Discovery module (audit Module 2)
builder.Services.AddScoped<IWorkPostService, WorkPostService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IPlatformService, PlatformService>();

// Add alongside the other service registrations:
builder.Services.AddScoped<IVendorService, VendorService>();

// Favorites module (audit Module 9)
builder.Services.AddScoped<IFavoriteService, FavoriteService>();

// Notifications module (audit Module 10)
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<INotificationPublisher, SignalRNotificationPublisher>();

// =========================================
// AutoMapper
// =========================================
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// =========================================
// API
// =========================================
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers()
    // Serialize enums (e.g. UserRole) as their string names ("Customer",
    // "Vendor", "Admin") instead of raw numbers — the frontend's
    // auth.service.ts maps AuthResponse.role via `.toLowerCase()`, which
    // throws on a numeric value.
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EventHub API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
            },
            Array.Empty<string>()
        }
    });
});

// =========================================
// Middleware
// =========================================
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS must come before auth (audit Module 1: enable CORS globally)
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Real-time push endpoint for Module 10 (Notifications).
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();