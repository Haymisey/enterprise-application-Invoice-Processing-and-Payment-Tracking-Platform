using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using InvoiceManagement.Application.Commands.CreateInvoice;
using MediatR;
using Shared.Infrastructure.Messaging;

namespace InvoiceManagement.Infrastructure.EventConsumers;

/// <summary>
/// Subscribes to ClassificationCompletedEvent from AI Classification module.
/// Automatically creates a draft invoice based on AI-extracted data.
/// </summary>
public sealed class InvoiceExtractedEventConsumer : RabbitMqConsumer
{
    private readonly ILogger<InvoiceExtractedEventConsumer> _logger;

    protected override string QueueName => "invoice-extracted-queue";
    
    protected override string[] SubscribedEventTypes => new[]
    {
        "ClassificationCompletedEvent"
    };

    public InvoiceExtractedEventConsumer(
        IServiceProvider serviceProvider,
        ILogger<InvoiceExtractedEventConsumer> logger,
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
        _logger.LogInformation("Processing {EventType} for draft invoice creation", eventType);

        try
        {
            var eventData = JsonSerializer.Deserialize<ClassificationCompletedEventDto>(eventPayload);
            
            if (eventData is null)
            {
                _logger.LogWarning("Failed to deserialize ClassificationCompletedEvent");
                return;
            }

            var mediator = serviceProvider.GetRequiredService<IMediator>();

            // For demo purposes: if a vendor name is extracted, we should ideally look up the VendorId.
            // If unknown, we'll use a placeholder or the first available vendor.
            // Here we'll default to a fixed Guid for the demo vendor if not provided.
            var demoVendorId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            var command = new CreateInvoiceCommand(
                eventData.InvoiceNumber ?? $"AI-{Guid.NewGuid().ToString().Substring(0, 8)}",
                demoVendorId, 
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(30),
                "AI-Assistant",
                $"Extracted from image. Confidence: {eventData.ConfidenceScore:P2}",
                new List<CreateInvoiceLineItemDto> 
                { 
                    new CreateInvoiceLineItemDto("AI Extracted Total", 1, eventData.TotalAmount ?? 0) 
                },
                eventData.ClassificationId);

            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Successfully created draft invoice {InvoiceId} from AI extraction",
                    result.Value);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to create draft invoice from AI extraction: {Error}",
                    result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AI Extraction event");
            throw; 
        }
    }
}

internal record ClassificationCompletedEventDto(
    Guid ClassificationId,
    string? InvoiceNumber,
    string? VendorName,
    decimal? TotalAmount,
    double ConfidenceScore);
