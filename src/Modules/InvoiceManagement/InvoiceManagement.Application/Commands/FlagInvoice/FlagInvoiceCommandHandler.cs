using InvoiceManagement.Domain.Repositories;
using InvoiceManagement.Domain.ValueObjects;
using Shared.Application.Messaging;
using Shared.Domain.Results;

namespace InvoiceManagement.Application.Commands.FlagInvoice;

internal sealed class FlagInvoiceCommandHandler : ICommandHandler<FlagInvoiceCommand>
{
    private readonly IInvoiceRepository _repository;
    private readonly IInvoiceUnitOfWork _unitOfWork;

    public FlagInvoiceCommandHandler(IInvoiceRepository repository, IInvoiceUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(FlagInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _repository.GetByIdAsync(InvoiceId.Create(request.InvoiceId), cancellationToken);

        if (invoice is null)
        {
            return Result.Failure(InvoiceErrors.NotFound(request.InvoiceId));
        }

        try
        {
            invoice.FlagForReview(request.Reason, request.FlaggedBy);
            _repository.Update(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(new Error("Invoice.FlagError", ex.Message));
        }
    }
}
