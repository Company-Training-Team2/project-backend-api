using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using EventHub.Application.Interfaces;
using EventHub.Application.Services;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using EventHub.Infrastructure.Persistence.Context;
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

app.Run();