using AIClassification.Domain.Repositories;
using AIClassification.Domain.ValueObjects;
using Shared.Application.Messaging;
using Shared.Domain.Results;

namespace AIClassification.Application.Queries.GetClassification;

internal sealed class GetClassificationQueryHandler 
    : IQueryHandler<GetClassificationQuery, ClassificationResponse?>
{
    private readonly IClassificationRepository _repository;

    public GetClassificationQueryHandler(IClassificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<ClassificationResponse?> Handle(
        GetClassificationQuery request, 
        CancellationToken cancellationToken)
    {
        var classification = await _repository.GetByIdAsync(
            ClassificationId.Create(request.Id), 
            cancellationToken);

        if (classification is null)
        {
            return null;
        }

        return new ClassificationResponse(
            classification.Id.Value,
            classification.ImageUrl,
            classification.Status.ToString(),
            classification.ExtractedData?.TotalAmount,
            classification.ExtractedData?.InvoiceNumber,
            classification.ConfidenceScore,
            classification.IsDuplicate,
            classification.IsFraudulent,
            classification.FraudReason,
            classification.ErrorMessage);
    }
}
