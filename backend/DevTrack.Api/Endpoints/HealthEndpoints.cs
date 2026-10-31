
namespace DevTrackAPI.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/health")
                       .WithTags("Health");

        group.MapGet("/", () =>
        {
            return Results.Ok(new
            {
                Status = "Healthy",
                Application = "DevTrack API",
                Timestamp = DateTime.UtcNow
            });
        });
    }
}
