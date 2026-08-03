using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using EventHub.Infrastructure.Persistence.Context;
using EventHub.Infrastructure.Persistence.Repositories;
using EventHub.Infrastructure.Persistence.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EventHub.Application.Interfaces;
using EventHub.Application.Services;

//using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);


// ── Persistence ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
<<<<<<< HEAD
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("EventHub.Infrastructure")));

=======
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("EventHub.Infrastructure"));

    // ⚠️ اضغط على الـ Warning ده مؤقتاً
    options.ConfigureWarnings(warnings =>
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});
>>>>>>> 9c5d494 (feat(auth): complete auth-user-schema (Task 1))
builder.Services.AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserService, UserService>();
//builder.Services.AddHttpContextAccessor();

// ── Application services (uncomment as feature branches merge) ────────────────
//builder.Services.AddScoped<IUserService, UserService>();
// builder.Services.AddScoped<IEventService, EventService>();
// builder.Services.AddScoped<IBookingService, BookingService>();
// builder.Services.AddScoped<IPaymentService, PaymentService>();
// builder.Services.AddScoped<IReviewService, ReviewService>();

// ── Auth (wire when identity branch merges) ───────────────────────────────────
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options => { ... });

// ── API ───────────────────────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "EventHub API", Version = "v1" });
});
builder.Services.AddAuthentication();

builder.Services.AddAuthorization();
var app = builder.Build();

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