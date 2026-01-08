using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InvoiceProcessingPlatform.API.Controllers;

/// <summary>
/// Authentication controller for user information and testing.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthController(IHttpClientFactory httpClientFactory, IConfiguration _configuration)
    {
        _httpClientFactory = httpClientFactory;
        this._configuration = _configuration;
    }

    /// <summary>
    /// Login and get JWT token from Keycloak.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var keycloakConfig = _configuration.GetSection("Keycloak");
        var authority = keycloakConfig["Authority"];
        var tokenEndpoint = $"{authority}/protocol/openid-connect/token";

        using var client = _httpClientFactory.CreateClient();

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", keycloakConfig["Audience"] ?? "invoice-api"),
            new KeyValuePair<string, string>("client_secret", keycloakConfig["ClientSecret"] ?? string.Empty),
            new KeyValuePair<string, string>("username", request.Username),
            new KeyValuePair<string, string>("password", request.Password),
            new KeyValuePair<string, string>("grant_type", "password")
        });

        var response = await client.PostAsync(tokenEndpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            return Unauthorized(new { message = "Invalid credentials or unauthorized client" });
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<TokenResponse>(jsonResponse);

        return Ok(tokenData);
    }

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

public record LoginRequest(string Username, string Password);

public record TokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("token_type")] string TokenType);
