
using DevTrackAPI.Data;
using DevTrackAPI.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register SQL Server with Entity Framework Core
builder.Services.AddDbContext<DevTrackDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString(
                "DefaultConnection"
            )
        )
);

var app = builder.Build();

// API endpoints
app.MapHealthEndpoints();

app.Run();
