using Shared.Application.Messaging;

namespace AIClassification.Application.Queries.GetClassification;

public record GetClassificationQuery(Guid Id) : IQuery<ClassificationResponse?>;

public record ClassificationResponse(
    Guid Id,
    string ImageUrl,
    string Status,
    decimal? TotalAmount,
    string? InvoiceNumber,
    double ConfidenceScore,
    bool IsDuplicate,
    bool IsFraudulent,
    string? FraudReason,
    string? ErrorMessage);
