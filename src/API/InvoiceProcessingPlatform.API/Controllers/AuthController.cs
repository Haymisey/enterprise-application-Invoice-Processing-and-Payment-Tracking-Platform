using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InvoiceProcessingPlatform.API.Controllers;

/// <summary>
/// Authentication controller for user information and testing.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    /// <summary>
    /// Get current user information from JWT token.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        var identity = User.Identity as ClaimsIdentity;
        
        if (identity == null || !identity.IsAuthenticated)
        {
            return Unauthorized();
        }

        var userInfo = new UserInfo
        {
            Username = User.FindFirst(ClaimTypes.Name)?.Value 
                ?? User.FindFirst("preferred_username")?.Value 
                ?? "Unknown",
            Email = User.FindFirst(ClaimTypes.Email)?.Value 
                ?? User.FindFirst("email")?.Value,
            Roles = User.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList(),
            Claims = User.Claims.Select(c => new ClaimInfo 
            { 
                Type = c.Type, 
                Value = c.Value 
            }).ToList()
        };

        return Ok(userInfo);
    }

    /// <summary>
    /// Health check endpoint (no authentication required).
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Test endpoint for admin role.
    /// </summary>
    [HttpGet("admin-only")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult AdminOnly()
    {
        return Ok(new { message = "You have admin access!" });
    }
}

public record UserInfo
{
    public string Username { get; init; } = string.Empty;
    public string? Email { get; init; }
    public List<string> Roles { get; init; } = new();
    public List<ClaimInfo> Claims { get; init; } = new();
}

public record ClaimInfo
{
    public string Type { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
}
