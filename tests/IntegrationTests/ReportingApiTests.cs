
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IntegrationTests;

public class ReportingApiTests : IClassFixture<WebApplicationFactory<InvoiceProcessingPlatform.API.Program>>
{
    private readonly WebApplicationFactory<InvoiceProcessingPlatform.API.Program> _factory;

    public ReportingApiTests(WebApplicationFactory<InvoiceProcessingPlatform.API.Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetSummary_ReturnsOk_AndValidShape()
    {
      
        var client = _factory.CreateClient();

     
        var response = await client.GetAsync("/api/reports/summary");

    
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        
        var summary = await response.Content.ReadFromJsonAsync(new
        {
            TotalInvoices = 0,
            TotalValue = 0m,
            ByStatus = new[]
            {
                new { Status = string.Empty, Count = 0, TotalAmount = 0m }
            },
            LastUpdated = DateTime.UtcNow
        });

        Assert.NotNull(summary);
        Assert.True(summary!.TotalInvoices >= 0);
        Assert.True(summary.TotalValue >= 0);
        Assert.NotNull(summary.ByStatus);
    }
}
