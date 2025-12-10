using Shared.Application.Messaging;

namespace PaymentTracking.Application.Commands.SchedulePayment;

/// <summary>
/// Command to schedule a payment for an approved invoice.
/// </summary>
public sealed record SchedulePaymentCommand(
    Guid InvoiceId,
    Guid VendorId,
    decimal Amount,
    string Currency,
    DateTime ScheduledDate,
    string CreatedBy) : ICommand<Guid>;
