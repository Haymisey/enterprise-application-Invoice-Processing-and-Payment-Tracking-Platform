using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IntegrationTests;

public class InvoiceApiTests : IClassFixture<WebApplicationFactory<InvoiceProcessingPlatform.API.Program>>
{
    private readonly WebApplicationFactory<InvoiceProcessingPlatform.API.Program> _factory;

    public InvoiceApiTests(WebApplicationFactory<InvoiceProcessingPlatform.API.Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Invoices_ReturnsOk()
    {
        
        var client = _factory.CreateClient();

        
        var response = await client.GetAsync("/api/invoices");

        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
