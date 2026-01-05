using Shared.Domain.Primitives;
using AIClassification.Domain.Events;
using AIClassification.Domain.ValueObjects;

namespace AIClassification.Domain.Aggregates;

public sealed class InvoiceClassification : AggregateRoot<ClassificationId>
{
    public string ImageUrl { get; private set; }
    public ClassificationStatus Status { get; private set; }
    public ExtractedInvoiceData? ExtractedData { get; private set; }
    public double ConfidenceScore { get; private set; }
    public bool IsDuplicate { get; private set; }
    public bool IsFraudulent { get; private set; }
    public string? FraudReason { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private InvoiceClassification(ClassificationId id, string imageUrl) : base(id)
    {
        ImageUrl = imageUrl;
        Status = ClassificationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    private InvoiceClassification() : base() 
    { 
        ImageUrl = string.Empty;
    }

    public static InvoiceClassification Create(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL is required", nameof(imageUrl));

        var classification = new InvoiceClassification(ClassificationId.Create(), imageUrl);
        
        classification.RaiseDomainEvent(new ClassificationStartedEvent(
            classification.Id.Value,
            imageUrl));

        return classification;
    }

    public void MarkAsProcessing()
    {
        Status = ClassificationStatus.Processing;
    }

    public void Complete(
        ExtractedInvoiceData extractedData,
        double confidenceScore,
        bool isDuplicate,
        bool isFraudulent,
        string? fraudReason = null)
    {
        ExtractedData = extractedData;
        ConfidenceScore = confidenceScore;
        IsDuplicate = isDuplicate;
        IsFraudulent = isFraudulent;
        FraudReason = fraudReason;
        CompletedAt = DateTime.UtcNow;

        if (isFraudulent)
        {
            Status = ClassificationStatus.FraudDetected;
            RaiseDomainEvent(new FraudDetectedEvent(Id.Value, fraudReason ?? "Unknown"));
        }
        else if (isDuplicate)
        {
            Status = ClassificationStatus.DuplicateDetected;
            RaiseDomainEvent(new DuplicateDetectedEvent(Id.Value, extractedData.InvoiceNumber ?? "Unknown"));
        }
        else
        {
            Status = ClassificationStatus.Completed;
            RaiseDomainEvent(new ClassificationCompletedEvent(
                Id.Value,
                extractedData.InvoiceNumber,
                extractedData.VendorName,
                extractedData.TotalAmount,
                confidenceScore));
        }
    }

    public void Fail(string errorMessage)
    {
        Status = ClassificationStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ClassificationFailedEvent(Id.Value, errorMessage));
    }
}