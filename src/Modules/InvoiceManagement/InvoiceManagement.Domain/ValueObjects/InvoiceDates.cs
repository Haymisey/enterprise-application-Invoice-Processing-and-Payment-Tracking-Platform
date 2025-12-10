using Shared.Domain.Primitives;

namespace InvoiceManagement.Domain.ValueObjects;

/// <summary>
/// Represents the date range for an invoice (issue date and due date).
/// </summary>
public sealed class InvoiceDates : ValueObject
{
    public DateTime IssueDate { get; }
    public DateTime DueDate { get; }

    private InvoiceDates(DateTime issueDate, DateTime dueDate)
    {
        if (dueDate < issueDate)
            throw new ArgumentException("Due date cannot be before issue date");

        IssueDate = issueDate;
        DueDate = dueDate;
    }

    public static InvoiceDates Create(DateTime issueDate, DateTime dueDate) => new(issueDate, dueDate);
    
    public static InvoiceDates CreateWithDefaultTerms(DateTime issueDate, int paymentTermDays = 30) 
        => new(issueDate, issueDate.AddDays(paymentTermDays));

    public bool IsOverdue() => DateTime.UtcNow > DueDate;

    public int DaysUntilDue => (DueDate - DateTime.UtcNow).Days;

    public int DaysOverdue => IsOverdue() ? (DateTime.UtcNow - DueDate).Days : 0;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return IssueDate;
        yield return DueDate;
    }

    public override string ToString() => $"Issued: {IssueDate:d}, Due: {DueDate:d}";
}
