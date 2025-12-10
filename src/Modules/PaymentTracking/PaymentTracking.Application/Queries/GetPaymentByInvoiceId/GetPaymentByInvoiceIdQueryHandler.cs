using PaymentTracking.Domain.Repositories;
using Shared.Application.Messaging;

namespace PaymentTracking.Application.Queries.GetPaymentByInvoiceId;

/// <summary>
/// Handler for GetPaymentByInvoiceIdQuery.
/// </summary>
internal sealed class GetPaymentByInvoiceIdQueryHandler : IQueryHandler<GetPaymentByInvoiceIdQuery, PaymentDto?>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentByInvoiceIdQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentDto?> Handle(GetPaymentByInvoiceIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByInvoiceIdAsync(request.InvoiceId, cancellationToken);

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
