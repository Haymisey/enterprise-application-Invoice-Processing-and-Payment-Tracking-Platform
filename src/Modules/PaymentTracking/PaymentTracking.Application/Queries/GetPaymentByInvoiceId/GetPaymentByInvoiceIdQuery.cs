using Shared.Application.Messaging;

namespace PaymentTracking.Application.Queries.GetPaymentByInvoiceId;

/// <summary>
/// Query to get a payment by invoice ID.
/// </summary>
public sealed record GetPaymentByInvoiceIdQuery(Guid InvoiceId) : IQuery<PaymentDto?>;
