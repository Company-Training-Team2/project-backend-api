using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using EventHub.API.Hubs;
using EventHub.API.RealTime;
using EventHub.Application.Helpers;
using EventHub.Application.Interfaces;
using EventHub.Application.Services;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using EventHub.Infrastructure.ExternalServices;
using EventHub.Infrastructure.Persistence.Context;
using EventHub.Infrastructure.Persistence.Repositories;
using EventHub.Infrastructure.Persistence.UnitOfWork;
using EventHub.Infrastructure.Services.AI;

// Force Microsoft.Data.SqlClient to use its fully-managed (pure C#) networking
// stack instead of the native SNI.dll. Some locked-down shared-hosting IIS
// environments lack the Visual C++ Redistributable that native SNI depends
// on, which crashes the whole worker process (w3wp.exe) instantly and
// silently — no managed exception, nothing in stdout — the moment any code
// path opens a SqlConnection. This switch avoids native SNI entirely.
AppContext.SetSwitch("Switch.Microsoft.Data.SqlClient.UseManagedNetworkingOnWindows", true);

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

    // TC_LOGIN_019: 5 consecutive failed password attempts used to be allowed
    // indefinitely (AuthService.LoginAsync called CheckPasswordSignInAsync with
    // lockoutOnFailure: false). Identity's own lockout tracker (AccessFailedCount
    // / LockoutEnd, already columns on AspNetUsers) does the job once enabled —
    // no separate counter needed. These values match Identity's own defaults;
    // spelled out explicitly so the 5-attempt/5-minute policy is documented,
    // not accidental.
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.AllowedForNewUsers = true;
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
    // handshake, so the token is sent as ?access_token=... instead.
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
// CORS
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
// In-process cache (used by AuthService for registration idempotency — REG-CUS-013)
// =========================================
builder.Services.AddMemoryCache();

// =========================================
// Repositories & Unit of Work
// =========================================
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IGuestRepository, GuestRepository>();

// =========================================
// Application Services
// =========================================
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IMfaService, MfaService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IWorkPostAvailabilityService, WorkPostAvailabilityService>();
builder.Services.AddScoped<IWorkPostService, WorkPostService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IPlatformService, PlatformService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();
builder.Services.AddScoped<IPayoutService, PayoutService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
// These three were implemented (see EventHub.Application/Services) but never
// registered here, so ChecklistController/DocumentsController/
// TimelineController would 500 with "Unable to resolve service for type
// I...Service" on every request — registering them for real.
builder.Services.AddScoped<IChecklistService, ChecklistService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<ITimelineService, TimelineService>();
builder.Services.AddScoped<IMessagingService, MessagingService>();

// =========================================
// Infrastructure Services
// =========================================
// AI Planner — IAIService contract lives in Application.Interfaces;
// MockAIService is the Infrastructure implementation.
// Swap for GeminiAIService (or any other provider) here without touching
// any controller or application-layer code.
builder.Services.AddScoped<IAIService, MockAIService>();

// Payment gateway (Paymob integration)
builder.Services.AddHttpClient<IPaymentGateway, PaymobPaymentGateway>();

// =========================================
// Real-time (SignalR)
// =========================================
builder.Services.AddSignalR();
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
// Middleware Pipeline
// =========================================
var app = builder.Build();

// Catches anything that escapes a controller/service unhandled. Without
// this, production requests that throw got a bare, bodyless 500 with
// nothing logged anywhere — impossible to tell "the DB is unreachable"
// from "a null ref in WorkPostService" from the outside. This wraps
// everything downstream, so it's registered first.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        logger.LogError(
            exceptionFeature?.Error,
            "Unhandled exception on {Method} {Path}",
            context.Request.Method,
            context.Request.Path);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(new
        {
            error = "An unexpected error occurred.",
            path = context.Request.Path.Value
        });
    });
});

// Was gated to Development only, so /swagger/index.html 404'd on the
// deployed (Production) backend. Enabled everywhere: this is a read-only
// API map (endpoint names, request/response shapes) - no different from the
// API reference PDF already handed to the mobile team - not a live data
// exposure, and every real endpoint still requires its own JWT auth.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// CORS must come before auth middleware
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Real-time push endpoint for Notifications module
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
