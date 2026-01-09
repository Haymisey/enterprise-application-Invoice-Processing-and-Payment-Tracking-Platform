using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IntegrationTests;

public class VendorsApiTests : IClassFixture<WebApplicationFactory<InvoiceProcessingPlatform.API.Program>>
{
    private readonly WebApplicationFactory<InvoiceProcessingPlatform.API.Program> _factory;

    public VendorsApiTests(WebApplicationFactory<InvoiceProcessingPlatform.API.Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetById_WhenVendorDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var unknownId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/vendors/{unknownId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Register_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Missing required fields (e.g., empty name and taxId)
        var body = new
        {
            Name = "",
            TaxId = "",
            Email = "invalid-email",   // invalid format
            Phone = "",
            ContactPerson = (string?)null,
            PaymentTermDays = 30
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/vendors", body);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Activate_WhenVendorDoesNotExist_ReturnsBadRequestOrNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var unknownId = Guid.NewGuid();
        var body = new { ActivatedBy = "tester" };

        // Act
        var response = await client.PostAsJsonAsync($"/api/vendors/{unknownId}/activate", body);

        // Assert
        Assert.True(
            response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound);
    }
}
