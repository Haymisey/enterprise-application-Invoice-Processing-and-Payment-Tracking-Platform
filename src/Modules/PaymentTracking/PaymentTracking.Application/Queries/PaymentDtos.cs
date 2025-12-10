using PaymentTracking.Domain.ValueObjects;

namespace PaymentTracking.Application.Queries;

/// <summary>
/// DTO for Payment data returned by queries.
/// </summary>
public sealed record PaymentDto(
    Guid Id,
    Guid InvoiceId,
    Guid VendorId,
    string Status,
    decimal Amount,
    string Currency,
    DateTime ScheduledDate,
    DateTime? ProcessedDate,
    DateTime? CompletedDate,
    string? TransactionReference,
    string? FailureReason,
    string CreatedBy,
    DateTime CreatedAt);

/// <summary>
/// Summary DTO for payment list views.
/// </summary>
public sealed record PaymentSummaryDto(
    Guid Id,
    Guid InvoiceId,
    string Status,
    decimal Amount,
    string Currency,
    DateTime ScheduledDate,
    bool IsOverdue);
