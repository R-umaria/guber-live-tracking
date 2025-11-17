using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Guber.CoordinatesApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }
    [HttpPost("token")]
    public IActionResult GetToken([FromBody] AuthRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return BadRequest(new { error = "UserId is required" });

        // Validate allowed roles
        var allowedRoles = new[] { "driver", "user" };
        var role = request.Role?.ToLower();

        if (string.IsNullOrWhiteSpace(role) || !allowedRoles.Contains(role))
            return BadRequest(new { error = "Invalid role. Allowed: 'driver', 'user'." });


        var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key missing");//check the key
        Console.WriteLine("JWT Key (generation): " + key);
        var issuer = _config["Jwt:Issuer"] ?? "Guber.LiveTracking";
        var audience = _config["Jwt:Audience"] ?? "Guber.LiveTracking";
        var duration = _config.GetValue<int?>("Jwt:AccessTokenDurationMinutes") ?? 30;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var identity = $"{role}:{request.UserId}".ToLower();

        // Define JWT claims: role, unique user identifier, subject, and token ID 
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.NameIdentifier, identity),
            new Claim(JwtRegisteredClaimNames.Sub, identity),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(duration),
            signingCredentials: credentials
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expires = token.ValidTo
        });
    }
}

public class AuthRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;//either a driver or a user
}
