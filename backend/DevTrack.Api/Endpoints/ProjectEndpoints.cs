
using DevTrackAPI.Data;
using DevTrackAPI.DTOs;
using DevTrackAPI.Models;
using DevTrackAPI.Validation;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DevTrackAPI.Endpoints;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects")
            .WithTags("Projects")
            .RequireAuthorization();

        // GET /api/projects?page=1&pageSize=10&search=api
        group.MapGet("/", async (
            ClaimsPrincipal user,
            DevTrackDbContext db,
            int? page,
            int? pageSize,
            string? search) =>
        {
            var userId = user.FindFirstValue("sub");

            if (userId is null)
                return Results.Unauthorized();

            if (!ProjectListQuery.TryCreate(
                    page, pageSize, search, out var query, out var error))
            {
                return Results.BadRequest(new { Message = error });
            }

            // Ownership is applied before both counting and pagination.
            var projectsQuery = db.Projects
                .AsNoTracking()
                .Where(p => p.OwnerId == userId);

            if (query.Search is not null)
            {
                projectsQuery = projectsQuery.Where(
                    p => p.Name.Contains(query.Search));
            }

            var totalCount = await projectsQuery.CountAsync();

            var projects = await projectsQuery
                .OrderBy(p => p.Id)
                .Skip(query.Skip)
                .Take(query.PageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.CreatedAt,
                    TaskCount = p.Tasks.Count
                })
                .ToListAsync();

            return Results.Ok(new
            {
                Items = projects,
                query.Page,
                query.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(
                    (double)totalCount / query.PageSize)
            });
        });

        // GET /api/projects/{id}
        group.MapGet("/{id:int}", async (
            int id,
            ClaimsPrincipal user,
            DevTrackDbContext db) =>
        {
            var userId = user.FindFirstValue("sub");

            if (userId is null)
                return Results.Unauthorized();

            var project = await db.Projects
                .AsNoTracking()
                .Where(p => p.Id == id &&
                            p.OwnerId == userId)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.CreatedAt,
                    TaskCount = p.Tasks.Count
                })
                .FirstOrDefaultAsync();

            return project is null
                ? Results.NotFound()
                : Results.Ok(project);
        });

        // POST /api/projects
        group.MapPost("/", async (
            CreateProjectDto dto,
            ClaimsPrincipal user,
            DevTrackDbContext db) =>
        {
            var userId = user.FindFirstValue("sub");

            if (userId is null)
                return Results.Unauthorized();

            var errors = RequestValidation.Validate(dto);

            if (string.IsNullOrWhiteSpace(dto.Name))
                errors["Name"] =
                    new[] { "Project name is required." };

            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var project = new Project
            {
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                OwnerId = userId
            };

            db.Projects.Add(project);
            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/projects/{project.Id}",
                new
                {
                    project.Id,
                    project.Name,
                    project.Description,
                    project.CreatedAt
                });
        });

        // PUT /api/projects/{id}
        group.MapPut("/{id:int}", async (
            int id,
            UpdateProjectDto dto,
            ClaimsPrincipal user,
            DevTrackDbContext db) =>
        {
            var userId = user.FindFirstValue("sub");

            if (userId is null)
                return Results.Unauthorized();

            var errors = RequestValidation.Validate(dto);

            if (string.IsNullOrWhiteSpace(dto.Name))
                errors["Name"] =
                    new[] { "Project name is required." };

            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var project = await db.Projects
                .FirstOrDefaultAsync(p =>
                    p.Id == id && p.OwnerId == userId);

            if (project is null)
                return Results.NotFound();

            project.Name = dto.Name.Trim();
            project.Description = dto.Description?.Trim();

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                project.Id,
                project.Name,
                project.Description,
                project.CreatedAt
            });
        });

        // DELETE /api/projects/{id}
        group.MapDelete("/{id:int}", async (
            int id,
            ClaimsPrincipal user,
            DevTrackDbContext db) =>
        {
            var userId = user.FindFirstValue("sub");

            if (userId is null)
                return Results.Unauthorized();

            var project = await db.Projects
                .FirstOrDefaultAsync(p =>
                    p.Id == id && p.OwnerId == userId);

            if (project is null)
                return Results.NotFound();

            var hasTasks = await db.WorkTasks
                .AnyAsync(t => t.ProjectId == id);

            if (hasTasks)
            {
                return Results.Conflict(new
                {
                    Message = "Cannot delete a project " +
                              "that contains tasks."
                });
            }

            db.Projects.Remove(project);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        // GET /api/projects/{id}/tasks
        group.MapGet("/{id:int}/tasks", async (
            int id,
            ClaimsPrincipal user,
            DevTrackDbContext db) =>
        {
            var userId = user.FindFirstValue("sub");

            if (userId is null)
                return Results.Unauthorized();

            var exists = await db.Projects.AnyAsync(
                p => p.Id == id && p.OwnerId == userId);

            if (!exists)
                return Results.NotFound();

            var tasks = await db.WorkTasks
                .AsNoTracking()
                .Where(t => t.ProjectId == id)
                .OrderBy(t => t.Id)
                .ToListAsync();

            return Results.Ok(tasks.Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                Status = t.Status.ToString(),
                t.CreatedAt,
                t.DueDate,
                t.ProjectId
            }));
        });
    }
}
