using PaymentTracking.Domain.Repositories;
using PaymentTracking.Domain.ValueObjects;
using Shared.Application.Messaging;

namespace PaymentTracking.Application.Queries.GetPaymentById;

/// <summary>
/// Handler for GetPaymentByIdQuery.
/// </summary>
internal sealed class GetPaymentByIdQueryHandler : IQueryHandler<GetPaymentByIdQuery, PaymentDto?>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentByIdQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var paymentId = PaymentId.Create(request.PaymentId);
        var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);

        if (payment is null)
        {
            return null;
        }

        return new PaymentDto(
            payment.Id.Value,
            payment.InvoiceId,
            payment.VendorId,
            payment.Status.ToString(),
            payment.Amount.Amount,
            payment.Amount.Currency,
            payment.ScheduledDate,
            payment.ProcessedDate,
            payment.CompletedDate,
            payment.TransactionReference,
            payment.FailureReason,
            payment.CreatedBy,
            payment.CreatedAt);
    }
}
