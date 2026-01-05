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
    private readonly IUnitOfWork _unitOfWork;

    public ClassifyInvoiceCommandHandler(
        IClassificationRepository repository,
        IGeminiService geminiService,
        IUnitOfWork unitOfWork)
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

        // Start async processing (in real app, use background job)
        _ = Task.Run(async () => await ProcessClassificationAsync(classification.Id.Value), cancellationToken);

        return Result.Success(classification.Id.Value);
    }

    private async Task ProcessClassificationAsync(Guid classificationId)
    {
        var classification = await _repository.GetByIdAsync(
            AIClassification.Domain.ValueObjects.ClassificationId.Create(classificationId));

        if (classification is null) return;

        try
        {
            classification.MarkAsProcessing();
            await _unitOfWork.SaveChangesAsync();

            // Extract data using Gemini
            var (extractedData, confidence) = await _geminiService.ExtractInvoiceDataAsync(classification.ImageUrl);

            // Check for duplicates (simplified - check if invoice number exists)
            var isDuplicate = extractedData.InvoiceNumber != null && 
                await _repository.ExistsWithInvoiceNumberAsync(extractedData.InvoiceNumber);

            // Detect fraud
            var (isFraudulent, fraudReason) = await _geminiService.DetectFraudAsync(extractedData);

            classification.Complete(extractedData, confidence, isDuplicate, isFraudulent, fraudReason);
        }
        catch (Exception ex)
        {
            classification.Fail(ex.Message);
        }

        _repository.Update(classification);
        await _unitOfWork.SaveChangesAsync();
    }
}