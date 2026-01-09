using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using InvoiceManagement.Application.Commands.FlagInvoice;
using MediatR;
using Shared.Infrastructure.Messaging;
using InvoiceManagement.Domain.Repositories;

namespace InvoiceManagement.Infrastructure.EventConsumers;

/// <summary>
/// Subscribes to FraudDetectedEvent from AI Classification module.
/// Automatically flags the corresponding invoice for manual review.
/// </summary>
public sealed class SuspiciousInvoiceEventConsumer : RabbitMqConsumer
{
    private readonly ILogger<SuspiciousInvoiceEventConsumer> _logger;

    protected override string QueueName => "suspicious-invoice-queue";
    
    protected override string[] SubscribedEventTypes => new[]
    {
        "FraudDetectedEvent"
    };

    public SuspiciousInvoiceEventConsumer(
        IServiceProvider serviceProvider,
        ILogger<SuspiciousInvoiceEventConsumer> logger,
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
        _logger.LogInformation("Processing {EventType} for invoice flagging", eventType);

        try
        {
            var eventData = JsonSerializer.Deserialize<FraudDetectedEventDto>(eventPayload);
            
            if (eventData is null)
            {
                _logger.LogWarning("Failed to deserialize FraudDetectedEvent");
                return;
            }

            // We need to find the invoice that was created for this classification
            // Because of eventual consistency, the InvoiceExtractedEventConsumer might still be working.
            // We'll retry a few times with a delay.
            var repository = serviceProvider.GetRequiredService<IInvoiceRepository>();
            InvoiceManagement.Domain.Aggregates.Invoice? invoice = null;
            
            for (int i = 0; i < 5; i++)
            {
                invoice = await repository.GetByClassificationIdAsync(eventData.ClassificationId, cancellationToken);
                if (invoice is not null) break;

                _logger.LogInformation("Invoice for classification {ClassificationId} not found yet. Retrying in 2s... (Attempt {Attempt}/5)", 
                    eventData.ClassificationId, i + 1);
                await Task.Delay(2000, cancellationToken);
            }

            if (invoice is null)
            {
                _logger.LogWarning("Invoice for classification {ClassificationId} not found after retries. Letting it requeue.", eventData.ClassificationId);
                throw new Exception($"Invoice for classification {eventData.ClassificationId} not found yet.");
            }

            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var command = new FlagInvoiceCommand(
                invoice.Id.Value,
                $"AI detected potential fraud: {eventData.Reason}",
                "AI-Fraud-Detection");

            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Successfully flagged invoice {InvoiceId} as suspicious",
                    invoice.Id.Value);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to flag invoice {InvoiceId}: {Error}",
                    invoice.Id.Value,
                    result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Suspicious Invoice event");
            throw; 
        }
    }
}

internal record FraudDetectedEventDto(
    Guid ClassificationId,
    string Reason);
