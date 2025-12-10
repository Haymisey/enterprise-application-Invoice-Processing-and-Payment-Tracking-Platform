using InvoiceManagement.Domain.Aggregates;
using InvoiceManagement.Domain.Repositories;
using InvoiceManagement.Domain.ValueObjects;
using Shared.Application.Messaging;
using Shared.Domain.Primitives;
using Shared.Domain.Results;

namespace InvoiceManagement.Application.Commands.CreateInvoice;

/// <summary>
/// Handler for CreateInvoiceCommand.
/// Creates a new invoice with line items and persists it.
/// </summary>
internal sealed class CreateInvoiceCommandHandler : ICommandHandler<CreateInvoiceCommand, Guid>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate invoice number
        if (await _invoiceRepository.ExistsWithInvoiceNumberAsync(request.InvoiceNumber, cancellationToken))
        {
            return Result.Failure<Guid>(InvoiceErrors.DuplicateInvoiceNumber(request.InvoiceNumber));
        }

        // Create the invoice aggregate
        var invoice = Invoice.Create(
            request.InvoiceNumber,
            request.VendorId,
            request.IssueDate,
            request.DueDate,
            request.CreatedBy,
            request.Notes);

        // Add line items
        foreach (var item in request.LineItems)
        {
            invoice.AddLineItem(item.Description, item.Quantity, item.UnitPrice);
        }

        // Persist
        await _invoiceRepository.AddAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(invoice.Id.Value);
    }
}
