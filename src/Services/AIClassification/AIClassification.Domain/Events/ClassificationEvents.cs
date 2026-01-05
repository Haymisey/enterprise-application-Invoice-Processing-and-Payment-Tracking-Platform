using Shared.Domain.Primitives;

namespace AIClassification.Domain.Events;

public sealed record ClassificationStartedEvent(
    Guid ClassificationId,
    string ImageUrl) : DomainEvent;

public sealed record ClassificationCompletedEvent(
    Guid ClassificationId,
    string? InvoiceNumber,
    string? VendorName,
    decimal? TotalAmount,
    double ConfidenceScore) : DomainEvent;

public sealed record FraudDetectedEvent(
    Guid ClassificationId,
    string Reason) : DomainEvent;

public sealed record DuplicateDetectedEvent(
    Guid ClassificationId,
    string InvoiceNumber) : DomainEvent;

public sealed record ClassificationFailedEvent(
    Guid ClassificationId,
    string ErrorMessage) : DomainEvent;