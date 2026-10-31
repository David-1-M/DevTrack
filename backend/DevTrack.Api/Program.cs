
using DevTrackAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Register API endpoints
app.MapHealthEndpoints();

app.Run();
