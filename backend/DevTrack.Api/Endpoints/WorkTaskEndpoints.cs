
using DevTrackAPI.Data;
using DevTrackAPI.DTOs;
using DevTrackAPI.Models;
using DevTrackAPI.Validation;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DevTrackAPI.Endpoints;

public static class WorkTaskEndpoints
{
    public static void MapWorkTaskEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks")
            .WithTags("Tasks")
            .RequireAuthorization();

        // GET /api/tasks/{id}
        group.MapGet("/{id:int}", async (
            int id,
            ClaimsPrincipal user,
            DevTrackDbContext db) =>
        {
            var userId = user.FindFirstValue("sub");

            if (userId is null)
                return Results.Unauthorized();

            var task = await db.WorkTasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.Project!.OwnerId == userId);

            if (task is null)
                return Results.NotFound();

            return Results.Ok(ToResponse(task));
        });

        // POST /api/tasks
        group.MapPost("/", async (
            CreateWorkTaskDto dto,
            ClaimsPrincipal user,
            DevTrackDbContext db) =>
        {
            var userId = user.FindFirstValue("sub");

            if (userId is null)
                return Results.Unauthorized();

            var errors = RequestValidation.Validate(dto);

            if (string.IsNullOrWhiteSpace(dto.Title))
                errors["Title"] =
                    new[] { "Task title is required." };

            if (!Enum.IsDefined(dto.Status))
                errors["Status"] =
                    new[] { "Invalid task status." };

            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var projectExists = await db.Projects
                .AnyAsync(p =>
                    p.Id == dto.ProjectId &&
                    p.OwnerId == userId);

            if (!projectExists)
                return Results.NotFound();

            var task = new WorkTask
            {
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                ProjectId = dto.ProjectId,
                Status = dto.Status,
                DueDate = dto.DueDate
            };

            db.WorkTasks.Add(task);
            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/tasks/{task.Id}",
                ToResponse(task));
        });

        // PUT /api/tasks/{id}
        group.MapPut("/{id:int}", async (
            int id,
            UpdateWorkTaskDto dto,
            ClaimsPrincipal user,
            DevTrackDbContext db) =>
        {
            var userId = user.FindFirstValue("sub");

            if (userId is null)
                return Results.Unauthorized();

            var errors = RequestValidation.Validate(dto);

            if (string.IsNullOrWhiteSpace(dto.Title))
                errors["Title"] =
                    new[] { "Task title is required." };

            if (!Enum.IsDefined(dto.Status))
                errors["Status"] =
                    new[] { "Invalid task status." };

            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var task = await db.WorkTasks
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.Project!.OwnerId == userId);

            if (task is null)
                return Results.NotFound();

            var destinationIsOwned = await db.Projects
                .AnyAsync(p =>
                    p.Id == dto.ProjectId &&
                    p.OwnerId == userId);

            if (!destinationIsOwned)
                return Results.NotFound();

            task.Title = dto.Title.Trim();
            task.Description = dto.Description?.Trim();
            task.ProjectId = dto.ProjectId;
            task.Status = dto.Status;
            task.DueDate = dto.DueDate;

            await db.SaveChangesAsync();

            return Results.Ok(ToResponse(task));
        });

        // DELETE /api/tasks/{id}
        group.MapDelete("/{id:int}", async (
            int id,
            ClaimsPrincipal user,
            DevTrackDbContext db) =>
        {
            var userId = user.FindFirstValue("sub");

            if (userId is null)
                return Results.Unauthorized();

            var task = await db.WorkTasks
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.Project!.OwnerId == userId);

            if (task is null)
                return Results.NotFound();

            db.WorkTasks.Remove(task);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }

    private static object ToResponse(WorkTask task)
    {
        return new
        {
            task.Id,
            task.Title,
            task.Description,
            Status = task.Status.ToString(),
            task.CreatedAt,
            task.DueDate,
            task.ProjectId
        };
    }
}
