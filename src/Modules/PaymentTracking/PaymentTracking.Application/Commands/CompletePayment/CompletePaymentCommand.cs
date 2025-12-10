using Shared.Application.Messaging;

namespace PaymentTracking.Application.Commands.CompletePayment;

/// <summary>
/// Command to mark a payment as completed.
/// </summary>
public sealed record CompletePaymentCommand(
    Guid PaymentId,
    string TransactionReference) : ICommand;
