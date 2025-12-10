namespace InvoiceManagement.Domain.ValueObjects;

/// <summary>
/// Represents the lifecycle status of an Invoice.
/// </summary>
public enum InvoiceStatus
{
    /// <summary>Invoice created but not yet submitted for approval.</summary>
    Draft = 0,

    /// <summary>Invoice submitted and awaiting review.</summary>
    Pending = 1,

    /// <summary>Invoice validated and approved for payment.</summary>
    Approved = 2,

    /// <summary>Invoice rejected during review.</summary>
    Rejected = 3,

    /// <summary>Payment has been completed for this invoice.</summary>
    Paid = 4,

    /// <summary>Invoice is past its due date and unpaid.</summary>
    Overdue = 5,

    /// <summary>Invoice has been cancelled.</summary>
    Cancelled = 6,

    /// <summary>Invoice flagged for manual review (suspicious/duplicate).</summary>
    FlaggedForReview = 7
}
