using Shared.Application.Messaging;

namespace PaymentTracking.Application.Queries.GetPaymentById;

/// <summary>
/// Query to get a payment by its ID.
/// </summary>
public sealed record GetPaymentByIdQuery(Guid PaymentId) : IQuery<PaymentDto?>;
