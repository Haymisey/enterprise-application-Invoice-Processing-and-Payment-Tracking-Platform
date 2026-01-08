using AIClassification.Domain.Aggregates;
using AIClassification.Domain.Repositories;
using AIClassification.Application.Services;
using Shared.Application.Messaging;
using Shared.Domain.Primitives;
using Shared.Domain.Results;

namespace AIClassification.Application.Commands.ClassifyInvoice;

internal sealed class ClassifyInvoiceCommandHandler : ICommandHandler<ClassifyInvoiceCommand, Guid>
{
    private readonly IClassificationRepository _repository;
    private readonly IGeminiService _geminiService;
    private readonly IAIUnitOfWork _unitOfWork;

    public ClassifyInvoiceCommandHandler(
        IClassificationRepository repository,
        IGeminiService geminiService,
        IAIUnitOfWork unitOfWork)
    {
        _repository = repository;
        _geminiService = geminiService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(ClassifyInvoiceCommand request, CancellationToken cancellationToken)
    {
        var classification = InvoiceClassification.Create(request.ImageUrl);
        await _repository.AddAsync(classification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(classification.Id.Value);
    }
}