
using DevTrackAPI.Data;
using DevTrackAPI.Endpoints;
using DevTrackAPI.Models;
using DevTrackAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. SQL Server and Entity Framework Core
builder.Services.AddDbContext<DevTrackDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

// 2. ASP.NET Core Identity
builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.User.RequireUniqueEmail = true;

        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole>()
    .AddSignInManager()
    .AddEntityFrameworkStores<DevTrackDbContext>()
    .AddDefaultTokenProviders();

// 3. JWT authentication
var jwtKey = builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException(
        "JWT signing key is not configured.");

var issuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException(
        "JWT issuer is missing.");

var audience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException(
        "JWT audience is missing.");

var signingKey = new SymmetricSecurityKey(
    Convert.FromBase64String(jwtKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,

                ValidateAudience = true,
                ValidAudience = audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),

                NameClaimType = "sub",
                RoleClaimType = "role"
            };
    });

// 4. Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

// 5. Register JWT service
builder.Services.AddScoped<JwtTokenService>();

var app = builder.Build();

// 6. Create User and Admin roles
await EnsureRolesAsync(app.Services);

// 7. Authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// 8. Register API endpoints
app.MapHealthEndpoints();
app.MapAuthEndpoints();
app.MapProjectEndpoints();
app.MapWorkTaskEndpoints();

// 9. Admin-only test endpoint
app.MapGet("/api/admin/status", () =>
    Results.Ok(new
    {
        Message = "Admin access granted."
    }))
    .RequireAuthorization("AdminOnly");

app.Run();

// 10. Create roles if they don't exist
static async Task EnsureRolesAsync(
    IServiceProvider services)
{
    using var scope = services.CreateScope();

    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    foreach (var role in new[] { "User", "Admin" })
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(
                new IdentityRole(role));

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not create role: {role}");
            }
        }
    }
}

public partial class Program { }
