using Shared.Domain.Results;

namespace PaymentTracking.Application.Commands;

/// <summary>
/// Domain-specific errors for Payment operations.
/// </summary>
public static class PaymentErrors
{
    public static Error NotFound(Guid paymentId) => 
        new("Payment.NotFound", $"Payment with ID '{paymentId}' was not found.");

    public static Error PaymentAlreadyExists(Guid invoiceId) => 
        new("Payment.AlreadyExists", $"Payment already exists for invoice '{invoiceId}'.");

    public static Error InvalidStatusTransition(string currentStatus, string action) => 
        new("Payment.InvalidStatus", $"Cannot {action} payment in {currentStatus} status.");

    public static Error AlreadyCompleted => 
        new("Payment.AlreadyCompleted", "Payment has already been completed.");

    public static Error ScheduledDateInPast => 
        new("Payment.InvalidDate", "Scheduled date cannot be in the past.");
}
