using Shared.Domain.Primitives;

namespace AIClassification.Domain.ValueObjects;

public sealed class ExtractedInvoiceData : ValueObject
{
    public string? InvoiceNumber { get; }
    public string? VendorName { get; }
    public decimal? TotalAmount { get; }
    public string? Currency { get; }
    public DateTime? IssueDate { get; }
    public DateTime? DueDate { get; }
    public List<string> LineItems { get; }

    private ExtractedInvoiceData(
        string? invoiceNumber,
        string? vendorName,
        decimal? totalAmount,
        string? currency,
        DateTime? issueDate,
        DateTime? dueDate,
        List<string> lineItems)
    {
        InvoiceNumber = invoiceNumber;
        VendorName = vendorName;
        TotalAmount = totalAmount;
        Currency = currency;
        IssueDate = issueDate;
        DueDate = dueDate;
        LineItems = lineItems ?? new List<string>();
    }

    public static ExtractedInvoiceData Create(
        string? invoiceNumber,
        string? vendorName,
        decimal? totalAmount,
        string? currency,
        DateTime? issueDate,
        DateTime? dueDate,
        List<string>? lineItems = null)
    {
        return new ExtractedInvoiceData(
            invoiceNumber,
            vendorName,
            totalAmount,
            currency,
            issueDate,
            dueDate,
            lineItems ?? new List<string>());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return InvoiceNumber ?? string.Empty;
        yield return VendorName ?? string.Empty;
        yield return TotalAmount ?? 0;
    }
}