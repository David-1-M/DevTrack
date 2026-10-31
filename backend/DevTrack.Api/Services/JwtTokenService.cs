
using DevTrackAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DevTrackAPI.Services;

public class JwtTokenService
{
    private readonly IConfiguration _config;
    private readonly UserManager<ApplicationUser> _users;

    public JwtTokenService(
        IConfiguration config,
        UserManager<ApplicationUser> users)
    {
        _config = config;
        _users = users;
    }

    public async Task<string> GenerateAsync(
        ApplicationUser user)
    {
        var roles = await _users.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new("sub", user.Id),
            new("email", user.Email ?? ""),
            new("jti", Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(
            role => new Claim("role", role)));

        var key = new SymmetricSecurityKey(
            Convert.FromBase64String(
                _config["Jwt:SigningKey"]!));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var expiry = int.Parse(
            _config["Jwt:ExpiryMinutes"] ?? "30");

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiry),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}
