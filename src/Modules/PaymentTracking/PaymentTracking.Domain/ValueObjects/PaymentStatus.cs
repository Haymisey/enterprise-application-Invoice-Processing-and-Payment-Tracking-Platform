namespace PaymentTracking.Domain.ValueObjects;

/// <summary>
/// Represents the lifecycle status of a Payment.
/// </summary>
public enum PaymentStatus
{
    /// <summary>Payment is scheduled but not yet processed.</summary>
    Scheduled = 0,

    /// <summary>Payment is currently being processed.</summary>
    Processing = 1,

    /// <summary>Payment has been successfully completed.</summary>
    Completed = 2,

    /// <summary>Payment failed during processing.</summary>
    Failed = 3,

    /// <summary>Payment was cancelled before processing.</summary>
    Cancelled = 4,

    /// <summary>Payment is overdue (past scheduled date).</summary>
    Overdue = 5
}
