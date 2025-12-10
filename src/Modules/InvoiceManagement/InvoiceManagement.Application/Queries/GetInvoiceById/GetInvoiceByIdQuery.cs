using Shared.Application.Messaging;

namespace InvoiceManagement.Application.Queries.GetInvoiceById;

/// <summary>
/// Query to get an invoice by its ID.
/// </summary>
public sealed record GetInvoiceByIdQuery(Guid InvoiceId) : IQuery<InvoiceDto?>;
