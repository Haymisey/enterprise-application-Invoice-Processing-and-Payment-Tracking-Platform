using PaymentTracking.Domain.Aggregates;
using PaymentTracking.Domain.Repositories;
using Shared.Application.Messaging;
using Shared.Domain.Primitives;
using Shared.Domain.Results;

namespace PaymentTracking.Application.Commands.SchedulePayment;

/// <summary>
/// Handler for SchedulePaymentCommand.
/// </summary>
internal sealed class SchedulePaymentCommandHandler : ICommandHandler<SchedulePaymentCommand, Guid>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SchedulePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
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

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(payment.Id.Value);
    }
}
