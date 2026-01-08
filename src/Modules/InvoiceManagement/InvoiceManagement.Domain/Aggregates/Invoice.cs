using Shared.Domain.Primitives;
using InvoiceManagement.Domain.Entities;
using InvoiceManagement.Domain.Events;
using InvoiceManagement.Domain.ValueObjects;

namespace InvoiceManagement.Domain.Aggregates;

/// <summary>
/// Invoice Aggregate Root.
/// Encapsulates the entire invoice lifecycle and enforces all business invariants.
/// All modifications to the invoice and its line items must go through this aggregate.
/// </summary>
public sealed class Invoice : AggregateRoot<InvoiceId>
{
    private readonly List<InvoiceLineItem> _lineItems = new();

    public string InvoiceNumber { get; private set; }
    public VendorId VendorId { get; private set; }
    public Guid? ClassificationId { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public InvoiceDates Dates { get; private set; }
    public Money SubTotal { get; private set; }
    public Money TaxAmount { get; private set; }
    public Money TotalAmount { get; private set; }
    public string? Notes { get; private set; }
    public string? RejectionReason { get; private set; }
    public string CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? ApprovedBy { get; private set; }

    public IReadOnlyCollection<InvoiceLineItem> LineItems => _lineItems.AsReadOnly();

    private Invoice(
        InvoiceId id,
        string invoiceNumber,
        VendorId vendorId,
        InvoiceDates dates,
        string createdBy,
        string? notes = null) : base(id)
    {
        InvoiceNumber = invoiceNumber;
        VendorId = vendorId;
        Dates = dates;
        Status = InvoiceStatus.Draft;
        SubTotal = Money.Zero();
        TaxAmount = Money.Zero();
        TotalAmount = Money.Zero();
        Notes = notes;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    // Required for EF Core
    private Invoice() : base()
    {
        InvoiceNumber = string.Empty;
        VendorId = VendorId.Create(Guid.Empty);
        Dates = InvoiceDates.CreateWithDefaultTerms(DateTime.UtcNow);
        SubTotal = Money.Zero();
        TaxAmount = Money.Zero();
        TotalAmount = Money.Zero();
        CreatedBy = string.Empty;
    }

    /// <summary>
    /// Factory method to create a new invoice.
    /// </summary>
    public static Invoice Create(
        string invoiceNumber,
        Guid vendorId,
        DateTime issueDate,
        DateTime dueDate,
        string createdBy,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new ArgumentException("Invoice number is required", nameof(invoiceNumber));
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("Created by is required", nameof(createdBy));

        var invoice = new Invoice(
            InvoiceId.Create(),
            invoiceNumber,
            VendorId.Create(vendorId),
            InvoiceDates.Create(issueDate, dueDate),
            createdBy,
            notes);

        invoice.RaiseDomainEvent(new InvoiceCreatedEvent(
            invoice.Id.Value,
            vendorId,
            invoiceNumber,
            0,
            "USD",
            issueDate,
            dueDate));

        return invoice;
    }

    /// <summary>
    /// Factory method to create an invoice from AI extraction.
    /// </summary>
    public static Invoice CreateFromExtraction(
        string invoiceNumber,
        Guid vendorId,
        DateTime issueDate,
        DateTime dueDate,
        decimal totalAmount,
        string currency,
        double confidenceScore,
        string extractedBy,
        Guid? classificationId = null)
    {
        var invoice = new Invoice(
            InvoiceId.Create(),
            invoiceNumber,
            VendorId.Create(vendorId),
            InvoiceDates.Create(issueDate, dueDate),
            extractedBy,
            $"AI Extracted with {confidenceScore:P0} confidence");

        invoice.ClassificationId = classificationId;
        invoice.TotalAmount = Money.Create(totalAmount, currency);

        invoice.RaiseDomainEvent(new InvoiceExtractedEvent(
            invoice.Id.Value,
            vendorId,
            invoiceNumber,
            totalAmount,
            currency,
            issueDate,
            dueDate,
            confidenceScore));

        return invoice;
    }

    /// <summary>
    /// Adds a line item to the invoice. Only allowed in Draft status.
    /// </summary>
    public void AddLineItem(string description, int quantity, decimal unitPrice, string currency = "USD")
    {
        EnsureStatus(InvoiceStatus.Draft, "add line items");

        var lineItem = InvoiceLineItem.Create(description, quantity, unitPrice, currency);
        _lineItems.Add(lineItem);
        RecalculateTotals();
    }

    /// <summary>
    /// Removes a line item from the invoice. Only allowed in Draft status.
    /// </summary>
    public void RemoveLineItem(Guid lineItemId)
    {
        EnsureStatus(InvoiceStatus.Draft, "remove line items");

        var lineItem = _lineItems.FirstOrDefault(li => li.Id.Value == lineItemId);
        if (lineItem is null)
            throw new InvalidOperationException($"Line item {lineItemId} not found");

        _lineItems.Remove(lineItem);
        RecalculateTotals();
    }

    /// <summary>
    /// Submits the invoice for approval. Transitions from Draft to Pending.
    /// </summary>
    public void Submit()
    {
        EnsureStatus(InvoiceStatus.Draft, "submit");
        
        if (!_lineItems.Any())
            throw new InvalidOperationException("Cannot submit invoice without line items");

        Status = InvoiceStatus.Pending;
    }

    /// <summary>
    /// Approves the invoice for payment. Transitions from Pending to Approved.
    /// </summary>
    public void Approve(string approvedBy)
    {
        if (string.IsNullOrWhiteSpace(approvedBy))
            throw new ArgumentException("Approved by is required", nameof(approvedBy));

        EnsureStatus(InvoiceStatus.Pending, "approve");

        Status = InvoiceStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedBy = approvedBy;

        RaiseDomainEvent(new InvoiceApprovedEvent(
            Id.Value,
            VendorId.Value,
            TotalAmount.Amount,
            TotalAmount.Currency,
            Dates.DueDate,
            approvedBy));
    }

    /// <summary>
    /// Rejects the invoice. Transitions from Pending to Rejected.
    /// </summary>
    public void Reject(string reason, string rejectedBy)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required", nameof(reason));
        if (string.IsNullOrWhiteSpace(rejectedBy))
            throw new ArgumentException("Rejected by is required", nameof(rejectedBy));

        EnsureStatus(InvoiceStatus.Pending, "reject");

        Status = InvoiceStatus.Rejected;
        RejectionReason = reason;

        RaiseDomainEvent(new InvoiceRejectedEvent(Id.Value, reason, rejectedBy));
    }

    /// <summary>
    /// Marks the invoice as paid. Transitions from Approved to Paid.
    /// </summary>
    public void MarkAsPaid(Guid paymentId)
    {
        EnsureStatus(InvoiceStatus.Approved, "mark as paid");

        Status = InvoiceStatus.Paid;

        RaiseDomainEvent(new InvoiceMarkedAsPaidEvent(Id.Value, paymentId, DateTime.UtcNow));
    }

    /// <summary>
    /// Flags the invoice for manual review.
    /// </summary>
    public void FlagForReview(string reason, string flaggedBy)
    {
        if (Status == InvoiceStatus.Paid || Status == InvoiceStatus.Cancelled)
            throw new InvalidOperationException($"Cannot flag invoice in {Status} status");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required", nameof(reason));

        Status = InvoiceStatus.FlaggedForReview;
        Notes = $"{Notes}\n[FLAGGED]: {reason} by {flaggedBy}";

        RaiseDomainEvent(new InvoiceFlaggedForReviewEvent(Id.Value, reason, flaggedBy));
    }

    /// <summary>
    /// Cancels the invoice.
    /// </summary>
    public void Cancel()
    {
        if (Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Cannot cancel a paid invoice");

        Status = InvoiceStatus.Cancelled;
    }

    /// <summary>
    /// Marks the invoice as overdue. Called by a background job.
    /// </summary>
    public void MarkAsOverdue()
    {
        if (Status == InvoiceStatus.Approved && Dates.IsOverdue())
        {
            Status = InvoiceStatus.Overdue;
        }
    }

    private void RecalculateTotals()
    {
        var currency = _lineItems.FirstOrDefault()?.UnitPrice.Currency ?? "USD";
        
        SubTotal = _lineItems.Aggregate(
            Money.Zero(currency),
            (sum, item) => sum.Add(item.TotalPrice));
        
        // Assuming 10% tax rate - this could be made configurable
        TaxAmount = SubTotal.Multiply(0.10m);
        TotalAmount = SubTotal.Add(TaxAmount);
    }

    private void EnsureStatus(InvoiceStatus expectedStatus, string action)
    {
        if (Status != expectedStatus)
            throw new InvalidOperationException(
                $"Cannot {action} when invoice is in {Status} status. Expected: {expectedStatus}");
    }
}
