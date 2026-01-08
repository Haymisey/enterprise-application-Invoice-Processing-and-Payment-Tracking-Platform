using InvoiceManagement.Domain.Repositories;
using InvoiceManagement.Domain.ValueObjects;
using Shared.Application.Messaging;
using Shared.Domain.Primitives;
using Shared.Domain.Results;

namespace InvoiceManagement.Application.Commands.SubmitInvoice;

/// <summary>
/// Handler for SubmitInvoiceCommand.
/// </summary>
internal sealed class SubmitInvoiceCommandHandler : ICommandHandler<SubmitInvoiceCommand>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IInvoiceUnitOfWork _unitOfWork;

    public SubmitInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IInvoiceUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SubmitInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoiceId = InvoiceId.Create(request.InvoiceId);
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);

        if (invoice is null)
        {
            return Result.Failure(InvoiceErrors.NotFound(request.InvoiceId));
        }

        try
        {
            invoice.Submit();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("Invoice.SubmitFailed", ex.Message));
        }

        _invoiceRepository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
