using PaymentTracking.Domain.Repositories;
using PaymentTracking.Domain.ValueObjects;
using Shared.Application.Messaging;
using Shared.Domain.Primitives;
using Shared.Domain.Results;

namespace PaymentTracking.Application.Commands.CompletePayment;

/// <summary>
/// Handler for CompletePaymentCommand.
/// </summary>
internal sealed class CompletePaymentCommandHandler : ICommandHandler<CompletePaymentCommand>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentUnitOfWork _unitOfWork;

    public CompletePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CompletePaymentCommand request, CancellationToken cancellationToken)
    {
        var paymentId = PaymentId.Create(request.PaymentId);
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);

        if (payment is null)
        {
            return Result.Failure(PaymentErrors.NotFound(request.PaymentId));
        }

        try
        {
            payment.Complete(request.TransactionReference);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("Payment.CompleteFailed", ex.Message));
        }

        _paymentRepository.Update(payment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
