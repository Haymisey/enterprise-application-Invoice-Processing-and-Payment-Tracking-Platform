using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AIClassification.Domain.Aggregates;
using AIClassification.Domain.Repositories;
using AIClassification.Application.Services;
using Shared.Infrastructure.Messaging;
using AIClassification.Domain.ValueObjects;

namespace AIClassification.Infrastructure.EventConsumers;

/// <summary>
/// Subscribes to ClassificationStartedEvent and performs the actual AI processing.
/// This ensures the processing happens in its own service scope.
/// </summary>
public sealed class ClassificationStartedEventConsumer : RabbitMqConsumer
{
    private readonly ILogger<ClassificationStartedEventConsumer> _logger;

    protected override string QueueName => "classification-started-queue";
    
    protected override string[] SubscribedEventTypes => new[]
    {
        "ClassificationStartedEvent"
    };

    public ClassificationStartedEventConsumer(
        IServiceProvider serviceProvider,
        ILogger<ClassificationStartedEventConsumer> logger,
        IConfiguration configuration)
        : base(serviceProvider, logger, configuration)
    {
        _logger = logger;
    }

    protected override async Task HandleEventAsync(
        string eventType,
        string eventPayload,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing {EventType} for AI Invoice Classification", eventType);

        try
        {
            var eventData = JsonSerializer.Deserialize<ClassificationStartedEventDto>(eventPayload);
            
            if (eventData is null)
            {
                _logger.LogWarning("Failed to deserialize ClassificationStartedEvent");
                return;
            }

            var repository = serviceProvider.GetRequiredService<IClassificationRepository>();
            var geminiService = serviceProvider.GetRequiredService<IGeminiService>();
            var unitOfWork = serviceProvider.GetRequiredService<IAIUnitOfWork>();

            var classification = await repository.GetByIdAsync(ClassificationId.Create(eventData.ClassificationId));

            if (classification is null)
            {
                _logger.LogWarning("Classification {ClassificationId} not found", eventData.ClassificationId);
                return;
            }

            classification.MarkAsProcessing();
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // 1. Extract data using Gemini
            _logger.LogInformation("Starting AI data extraction for classification {ClassificationId}", classification.Id.Value);
            var (extractedData, confidence) = await geminiService.ExtractInvoiceDataAsync(classification.ImageUrl);

            // 2. Check for duplicates (simplified)
            var isDuplicate = extractedData.InvoiceNumber != null && 
                await repository.ExistsWithInvoiceNumberAsync(extractedData.InvoiceNumber);

            // 3. Detect fraud
            _logger.LogInformation("Running fraud detection for classification {ClassificationId}", classification.Id.Value);
            var (isFraudulent, fraudReason) = await geminiService.DetectFraudAsync(extractedData);

            // 4. Complete classification
            classification.Complete(extractedData, confidence, isDuplicate, isFraudulent, fraudReason);
            
            repository.Update(classification);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully completed AI classification {ClassificationId}", classification.Id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ClassificationStartedEvent");
            // We should attempt to mark the classification as failed if we have the Ids
            throw; 
        }
    }
}

internal record ClassificationStartedEventDto(Guid ClassificationId, string ImageUrl);
