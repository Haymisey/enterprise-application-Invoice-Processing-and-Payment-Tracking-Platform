using InvoiceManagement.Domain.Aggregates;
using InvoiceManagement.Domain.Repositories;
using InvoiceManagement.Domain.ValueObjects;
using Shared.Application.Messaging;
using Shared.Domain.Results;

namespace InvoiceManagement.Application.Commands.CreateInvoice;

/// <summary>
/// Handler for CreateInvoiceCommand.
/// Creates a new invoice with line items and persists it.
/// </summary>
internal sealed class CreateInvoiceCommandHandler : ICommandHandler<CreateInvoiceCommand, Guid>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public CreateInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
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

        Console.WriteLine($"[INVOICE DEBUG] Created invoice with ID: {invoice.Id.Value}");

        // Persist - AddAsync now saves automatically to the correct InvoiceDbContext
        await _invoiceRepository.AddAsync(invoice, cancellationToken);
        Console.WriteLine($"[INVOICE DEBUG] Invoice added and saved to InvoiceDbContext");

        return Result.Success(invoice.Id.Value);
    }
}
