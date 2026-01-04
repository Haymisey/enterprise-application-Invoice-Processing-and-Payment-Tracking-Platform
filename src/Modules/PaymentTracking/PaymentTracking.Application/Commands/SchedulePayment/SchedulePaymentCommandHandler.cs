using PaymentTracking.Domain.Aggregates;
using PaymentTracking.Domain.Repositories;
using Shared.Application.Messaging;
using Shared.Domain.Results;

namespace PaymentTracking.Application.Commands.SchedulePayment;

/// <summary>
/// Handler for SchedulePaymentCommand.
/// </summary>
internal sealed class SchedulePaymentCommandHandler : ICommandHandler<SchedulePaymentCommand, Guid>
{
    private readonly IPaymentRepository _paymentRepository;

    public SchedulePaymentCommandHandler(
        IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<Guid>> Handle(SchedulePaymentCommand request, CancellationToken cancellationToken)
    {
        // Check if payment already exists for this invoice
        if (await _paymentRepository.ExistsForInvoiceAsync(request.InvoiceId, cancellationToken))
        {
            return Result.Failure<Guid>(PaymentErrors.PaymentAlreadyExists(request.InvoiceId));
        }

        var payment = Payment.Schedule(
            request.InvoiceId,
            request.VendorId,
            request.Amount,
            request.Currency,
            request.ScheduledDate,
            request.CreatedBy);

        Console.WriteLine($"[PAYMENT DEBUG] Created payment with ID: {payment.Id.Value}");

        // AddAsync now saves automatically to the correct PaymentDbContext
        await _paymentRepository.AddAsync(payment, cancellationToken);
        Console.WriteLine($"[PAYMENT DEBUG] Payment added and saved to PaymentDbContext");

        return Result.Success(payment.Id.Value);
    }
}
