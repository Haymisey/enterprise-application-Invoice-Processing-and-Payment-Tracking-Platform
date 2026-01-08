namespace Reporting.Domain.Entities;

public sealed class InvoiceSummary
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
}

public sealed class VendorSpendingSummary
{
    public Guid VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public int InvoiceCount { get; set; }
}
