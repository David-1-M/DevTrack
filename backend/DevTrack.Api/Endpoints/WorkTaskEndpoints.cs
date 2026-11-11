
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

        // GET /api/tasks?status=InProgress&projectId=1&sortBy=dueDate&sortOrder=asc
        group.MapGet("/", async (
            ClaimsPrincipal user,
            DevTrackDbContext db,
            string? status,
            int? projectId,
            string? sortBy,
            string? sortOrder,
            int? page,
            int? pageSize) =>
        {
            var userId = user.FindFirstValue("sub");

            if (userId is null)
                return Results.Unauthorized();

            if (!TaskListQuery.TryCreate(
                    status, projectId, sortBy, sortOrder,
                    page, pageSize, out var query, out var error))
            {
                return Results.BadRequest(new { Message = error });
            }

            // Scope the query to the current owner before filtering or counting.
            var tasksQuery = db.WorkTasks
                .AsNoTracking()
                .Where(t => t.Project!.OwnerId == userId);

            if (query.Status.HasValue)
                tasksQuery = tasksQuery.Where(t => t.Status == query.Status.Value);

            if (query.ProjectId.HasValue)
                tasksQuery = tasksQuery.Where(t => t.ProjectId == query.ProjectId.Value);

            var totalCount = await tasksQuery.CountAsync();

            var ordered = query.SortBy switch
            {
                "dueDate" when query.Descending =>
                    tasksQuery.OrderBy(t => t.DueDate == null)
                        .ThenByDescending(t => t.DueDate)
                        .ThenBy(t => t.Id),
                "dueDate" =>
                    tasksQuery.OrderBy(t => t.DueDate == null)
                        .ThenBy(t => t.DueDate)
                        .ThenBy(t => t.Id),
                _ when query.Descending => tasksQuery.OrderByDescending(t => t.CreatedAt)
                    .ThenBy(t => t.Id),
                _ => tasksQuery.OrderBy(t => t.CreatedAt)
                    .ThenBy(t => t.Id)
            };

            var tasks = await ordered
                .Skip(query.Skip)
                .Take(query.PageSize)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    Status = t.Status.ToString(),
                    t.CreatedAt,
                    t.DueDate,
                    t.ProjectId
                })
                .ToListAsync();

            return Results.Ok(new
            {
                Items = tasks,
                query.Page,
                query.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(
                    (double)totalCount / query.PageSize)
            });
        });

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
