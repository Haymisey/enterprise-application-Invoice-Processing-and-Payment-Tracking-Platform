using Shared.Domain.Primitives;
using PaymentTracking.Domain.Events;
using PaymentTracking.Domain.ValueObjects;

namespace PaymentTracking.Domain.Aggregates;

/// <summary>
/// Payment Aggregate Root.
/// Manages the lifecycle of a payment from scheduling through completion.
/// Payments are created when invoices are approved.
/// </summary>
public sealed class Payment : AggregateRoot<PaymentId>
{
    public Guid InvoiceId { get; private set; }
    public Guid VendorId { get; private set; }
    public PaymentAmount Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime ScheduledDate { get; private set; }
    public DateTime? ProcessedDate { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public string? TransactionReference { get; private set; }
    public string? FailureReason { get; private set; }
    public string CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Payment(
        PaymentId id,
        Guid invoiceId,
        Guid vendorId,
        PaymentAmount amount,
        DateTime scheduledDate,
        string createdBy) : base(id)
    {
        InvoiceId = invoiceId;
        VendorId = vendorId;
        Amount = amount;
        Status = PaymentStatus.Scheduled;
        ScheduledDate = scheduledDate;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
    }

    // Required for EF Core
    private Payment() : base()
    {
        Amount = PaymentAmount.Zero();
        CreatedBy = string.Empty;
    }

    /// <summary>
    /// Factory method to schedule a new payment for an approved invoice.
    /// </summary>
    public static Payment Schedule(
        Guid invoiceId,
        Guid vendorId,
        decimal amount,
        string currency,
        DateTime scheduledDate,
        string createdBy)
    {
        if (scheduledDate < DateTime.UtcNow.Date)
            throw new ArgumentException("Scheduled date cannot be in the past", nameof(scheduledDate));

        var payment = new Payment(
            PaymentId.Create(),
            invoiceId,
            vendorId,
            PaymentAmount.Create(amount, currency),
            scheduledDate,
            createdBy);

        payment.RaiseDomainEvent(new PaymentScheduledEvent(
            payment.Id.Value,
            invoiceId,
            vendorId,
            amount,
            currency,
            scheduledDate));

        return payment;
    }

    /// <summary>
    /// Start processing the payment.
    /// </summary>
    public void StartProcessing()
    {
        EnsureStatus(PaymentStatus.Scheduled, "start processing");
        Status = PaymentStatus.Processing;
        ProcessedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark the payment as completed.
    /// </summary>
    public void Complete(string transactionReference)
    {
        if (string.IsNullOrWhiteSpace(transactionReference))
            throw new ArgumentException("Transaction reference is required", nameof(transactionReference));

        if (Status != PaymentStatus.Processing && Status != PaymentStatus.Scheduled)
            throw new InvalidOperationException($"Cannot complete payment in {Status} status");

        Status = PaymentStatus.Completed;
        CompletedDate = DateTime.UtcNow;
        TransactionReference = transactionReference;

        RaiseDomainEvent(new PaymentCompletedEvent(
            Id.Value,
            InvoiceId,
            VendorId,
            Amount.Amount,
            Amount.Currency,
            CompletedDate.Value,
            transactionReference));
    }

    /// <summary>
    /// Mark the payment as failed.
    /// </summary>
    public void Fail(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Failure reason is required", nameof(reason));

        if (Status != PaymentStatus.Processing && Status != PaymentStatus.Scheduled)
            throw new InvalidOperationException($"Cannot fail payment in {Status} status");

        Status = PaymentStatus.Failed;
        FailureReason = reason;

        RaiseDomainEvent(new PaymentFailedEvent(Id.Value, InvoiceId, reason));
    }

    /// <summary>
    /// Cancel the payment.
    /// </summary>
    public void Cancel()
    {
        if (Status == PaymentStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed payment");

        Status = PaymentStatus.Cancelled;
    }

    /// <summary>
    /// Mark as overdue. Called by background job.
    /// </summary>
    public void MarkAsOverdue()
    {
        if (Status == PaymentStatus.Scheduled && ScheduledDate < DateTime.UtcNow.Date)
        {
            Status = PaymentStatus.Overdue;
            var daysOverdue = (DateTime.UtcNow.Date - ScheduledDate).Days;

            RaiseDomainEvent(new PaymentOverdueEvent(
                Id.Value,
                InvoiceId,
                ScheduledDate,
                daysOverdue));
        }
    }

    /// <summary>
    /// Reschedule an overdue or scheduled payment.
    /// </summary>
    public void Reschedule(DateTime newScheduledDate)
    {
        if (Status != PaymentStatus.Scheduled && Status != PaymentStatus.Overdue && Status != PaymentStatus.Failed)
            throw new InvalidOperationException($"Cannot reschedule payment in {Status} status");

        if (newScheduledDate < DateTime.UtcNow.Date)
            throw new ArgumentException("New scheduled date cannot be in the past", nameof(newScheduledDate));

        ScheduledDate = newScheduledDate;
        Status = PaymentStatus.Scheduled;
        FailureReason = null;
    }

    private void EnsureStatus(PaymentStatus expectedStatus, string action)
    {
        if (Status != expectedStatus)
            throw new InvalidOperationException(
                $"Cannot {action} when payment is in {Status} status. Expected: {expectedStatus}");
    }
}
