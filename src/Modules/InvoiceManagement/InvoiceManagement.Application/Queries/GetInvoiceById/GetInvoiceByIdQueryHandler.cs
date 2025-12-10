using InvoiceManagement.Domain.Repositories;
using InvoiceManagement.Domain.ValueObjects;
using Shared.Application.Messaging;

namespace InvoiceManagement.Application.Queries.GetInvoiceById;

/// <summary>
/// Handler for GetInvoiceByIdQuery.
/// </summary>
internal sealed class GetInvoiceByIdQueryHandler : IQueryHandler<GetInvoiceByIdQuery, InvoiceDto?>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoiceByIdQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<InvoiceDto?> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoiceId = InvoiceId.Create(request.InvoiceId);
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);

        if (invoice is null)
        {
            return null;
        }

        return new InvoiceDto(
            invoice.Id.Value,
            invoice.InvoiceNumber,
            invoice.VendorId.Value,
            "Unknown Vendor", // TODO: Fetch from VendorManagement BC
            invoice.Status.ToString(),
            invoice.Dates.IssueDate,
            invoice.Dates.DueDate,
            invoice.SubTotal.Amount,
            invoice.TaxAmount.Amount,
            invoice.TotalAmount.Amount,
            invoice.TotalAmount.Currency,
            invoice.Notes,
            invoice.CreatedBy,
            invoice.CreatedAt,
            invoice.ApprovedBy,
            invoice.ApprovedAt,
            invoice.LineItems.Select(li => new InvoiceLineItemDto(
                li.Id.Value,
                li.Description,
                li.Quantity,
                li.UnitPrice.Amount,
                li.TotalPrice.Amount)).ToList());
    }
}
