using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AIClassification.API.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IntegrationTests;

public class AIClassificationApiTests : IClassFixture<WebApplicationFactory<ClassificationsController>>
{
    private readonly WebApplicationFactory<ClassificationsController> _factory;

    public AIClassificationApiTests(WebApplicationFactory<ClassificationsController> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetClassification_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var id = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/classifications/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ClassifyInvoice_WhenImageUrlIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var body = new { ImageUrl = "" };

        // Act
        var response = await client.PostAsJsonAsync("/api/classifications", body);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
