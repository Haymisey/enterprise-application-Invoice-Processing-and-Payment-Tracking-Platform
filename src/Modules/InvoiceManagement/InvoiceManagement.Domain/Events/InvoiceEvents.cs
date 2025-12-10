using Shared.Domain.Primitives;
using InvoiceManagement.Domain.ValueObjects;

namespace InvoiceManagement.Domain.Events;

/// <summary>
/// Raised when a new invoice is created.
/// </summary>
public sealed record InvoiceCreatedEvent(
    Guid InvoiceId,
    Guid VendorId,
    string InvoiceNumber,
    decimal TotalAmount,
    string Currency,
    DateTime IssueDate,
    DateTime DueDate) : DomainEvent;

/// <summary>
/// Raised when an invoice is approved for payment.
/// </summary>
public sealed record InvoiceApprovedEvent(
    Guid InvoiceId,
    Guid VendorId,
    decimal TotalAmount,
    string Currency,
    DateTime DueDate,
    string ApprovedBy) : DomainEvent;

/// <summary>
/// Raised when an invoice is rejected.
/// </summary>
public sealed record InvoiceRejectedEvent(
    Guid InvoiceId,
    string Reason,
    string RejectedBy) : DomainEvent;

/// <summary>
/// Raised when an invoice is marked as paid.
/// </summary>
public sealed record InvoiceMarkedAsPaidEvent(
    Guid InvoiceId,
    Guid PaymentId,
    DateTime PaidDate) : DomainEvent;

/// <summary>
/// Raised when an invoice is flagged for manual review (suspicious/duplicate).
/// </summary>
public sealed record InvoiceFlaggedForReviewEvent(
    Guid InvoiceId,
    string Reason,
    string FlaggedBy) : DomainEvent;

/// <summary>
/// Raised when AI extracts data from an uploaded invoice document.
/// </summary>
public sealed record InvoiceExtractedEvent(
    Guid InvoiceId,
    Guid VendorId,
    string InvoiceNumber,
    decimal TotalAmount,
    string Currency,
    DateTime IssueDate,
    DateTime DueDate,
    double ConfidenceScore) : DomainEvent;
