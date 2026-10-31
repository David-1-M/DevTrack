
using DevTrackAPI.DTOs;
using DevTrackAPI.Models;
using DevTrackAPI.Services;
using DevTrackAPI.Validation;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace DevTrackAPI.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        // POST /api/auth/register
        group.MapPost("/register", async (
            RegisterUserDto dto,
            UserManager<ApplicationUser> users) =>
        {
            var errors = RequestValidation.Validate(dto);

            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var email = dto.Email.Trim();

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email
            };

            var result = await users.CreateAsync(
                user, dto.Password);

            if (!result.Succeeded)
            {
                return Results.ValidationProblem(
                    result.Errors
                        .GroupBy(e => "Registration")
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.Description)
                                  .ToArray()));
            }

            await users.AddToRoleAsync(user, "User");

            return Results.Created(
                $"/api/auth/me",
                new
                {
                    user.Id,
                    user.Email
                });
        });

        // POST /api/auth/login
        group.MapPost("/login", async (
            LoginUserDto dto,
            UserManager<ApplicationUser> users,
            SignInManager<ApplicationUser> signIn,
            JwtTokenService tokens) =>
        {
            var errors = RequestValidation.Validate(dto);

            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var user = await users.FindByEmailAsync(
                dto.Email.Trim());

            if (user is null)
                return Results.Unauthorized();

            var result = await signIn.CheckPasswordSignInAsync(
                user, dto.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
                return Results.Unauthorized();

            var token = await tokens.GenerateAsync(user);

            return Results.Ok(new
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresInSeconds = 1800
            });
        });

        // GET /api/auth/me
        group.MapGet("/me", async (
            ClaimsPrincipal principal,
            UserManager<ApplicationUser> users) =>
        {
            var userId = principal.FindFirstValue("sub");

            if (userId is null)
                return Results.Unauthorized();

            var user = await users.FindByIdAsync(userId);

            if (user is null)
                return Results.Unauthorized();

            return Results.Ok(new
            {
                user.Id,
                user.Email,
                Roles = await users.GetRolesAsync(user)
            });
        })
        .RequireAuthorization();
    }
}
