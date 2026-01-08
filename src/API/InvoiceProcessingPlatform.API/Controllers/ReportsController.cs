using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reporting.Infrastructure.Persistence;

namespace InvoiceProcessingPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ReportingDbContext _context;

    public ReportsController(ReportingDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get a high-level summary of all invoices and their financial status.
    /// This serves as the "Executive Dashboard" data.
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _context.InvoiceSummaries.ToListAsync();
        
        var dashboard = new
        {
            TotalInvoices = summary.Sum(s => s.Count),
            TotalValue = summary.Sum(s => s.TotalAmount),
            ByStatus = summary.Select(s => new
            {
                s.Status,
                s.Count,
                s.TotalAmount
            }),
            LastUpdated = DateTime.UtcNow
        };

        return Ok(dashboard);
    }
}
