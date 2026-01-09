using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using InvoiceProcessingPlatform.API;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IntegrationTests;

public class AuthApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_ReturnsOk_WithStatusField()
    {
        
        var client = _factory.CreateClient();

        
        var response = await client.GetAsync("/api/auth/health");

      
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(body);
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
      
        var client = _factory.CreateClient();

        
        var response = await client.GetAsync("/api/auth/me");

  
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminOnly_WithoutToken_ReturnsUnauthorized()
    {
       
        var client = _factory.CreateClient();

      
        var response = await client.GetAsync("/api/auth/admin-only");

     
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
