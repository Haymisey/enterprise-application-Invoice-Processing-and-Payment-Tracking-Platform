namespace AIClassification.Domain.ValueObjects;

public enum ClassificationStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    FraudDetected = 4,
    DuplicateDetected = 5
}