
using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IntegrationTests;

public class PaymentsApiTests : IClassFixture<WebApplicationFactory<InvoiceProcessingPlatform.API.Program>>
{
    private readonly WebApplicationFactory<InvoiceProcessingPlatform.API.Program> _factory;

    public PaymentsApiTests(WebApplicationFactory<InvoiceProcessingPlatform.API.Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetById_WhenPaymentDoesNotExist_ReturnsNotFound()
    {
        
        var client = _factory.CreateClient();
        var unknownId = Guid.NewGuid();

        
        var response = await client.GetAsync($"/api/payments/{unknownId}");

      
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Schedule_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        
        var client = _factory.CreateClient();

        
        var body = new
        {
            InvoiceId = Guid.NewGuid(),
            VendorId = Guid.NewGuid(),
            Amount = 0m,
            Currency = "USD",
            ScheduledDate = DateTime.UtcNow.Date
        };

        
        var response = await client.PostAsJsonAsync("/api/payments", body);

      
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
