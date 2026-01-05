using AIClassification.Domain.ValueObjects;

namespace AIClassification.Application.Services;

public interface IGeminiService
{
    Task<(ExtractedInvoiceData data, double confidence)> ExtractInvoiceDataAsync(
        string imageUrl, 
        CancellationToken cancellationToken = default);
    
    Task<(bool isFraudulent, string? reason)> DetectFraudAsync(
        ExtractedInvoiceData data, 
        CancellationToken cancellationToken = default);
}