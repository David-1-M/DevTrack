var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        Status = "Healthy",
        Application = "DevTrack API",
        Timestamp = DateTime.UtcNow
    });
});

app.Run();