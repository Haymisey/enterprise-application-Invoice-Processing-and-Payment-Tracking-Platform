using Shared.Domain.Primitives;

namespace PaymentTracking.Domain.Events;

/// <summary>
/// Raised when a payment is scheduled for an approved invoice.
/// </summary>
public sealed record PaymentScheduledEvent(
    Guid PaymentId,
    Guid InvoiceId,
    Guid VendorId,
    decimal Amount,
    string Currency,
    DateTime ScheduledDate) : DomainEvent;

/// <summary>
/// Raised when a payment is successfully completed.
/// </summary>
public sealed record PaymentCompletedEvent(
    Guid PaymentId,
    Guid InvoiceId,
    Guid VendorId,
    decimal Amount,
    string Currency,
    DateTime CompletedDate,
    string TransactionReference) : DomainEvent;

/// <summary>
/// Raised when a payment fails.
/// </summary>
public sealed record PaymentFailedEvent(
    Guid PaymentId,
    Guid InvoiceId,
    string FailureReason) : DomainEvent;

/// <summary>
/// Raised when a payment becomes overdue.
/// </summary>
public sealed record PaymentOverdueEvent(
    Guid PaymentId,
    Guid InvoiceId,
    DateTime ScheduledDate,
    int DaysOverdue) : DomainEvent;
