using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaymentTracking.Application.Commands.SchedulePayment;
using MediatR;
using Shared.Infrastructure.Messaging;

namespace PaymentTracking.Infrastructure.EventConsumers;

/// <summary>
/// Subscribes to InvoiceApprovedEvent from Invoice Management module.
/// When an invoice is approved, automatically schedules a payment for it.
/// </summary>
public sealed class InvoiceApprovedEventConsumer : RabbitMqConsumer
{
    private readonly ILogger<InvoiceApprovedEventConsumer> _logger;

    protected override string QueueName => "payment-tracking-queue";
    
    protected override string[] SubscribedEventTypes => new[]
    {
        "InvoiceApprovedEvent"  // Short name as used by the interceptor
    };

    public InvoiceApprovedEventConsumer(
        IServiceProvider serviceProvider,
        ILogger<InvoiceApprovedEventConsumer> logger,
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
        _logger.LogInformation("Processing {EventType} for payment scheduling", eventType);

        try
        {
            // Deserialize the event
            var eventData = JsonSerializer.Deserialize<InvoiceApprovedEventDto>(eventPayload);
            
            if (eventData is null)
            {
                _logger.LogWarning("Failed to deserialize InvoiceApprovedEvent");
                return;
            }

            // Get MediatR and send command to schedule payment
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            
            var command = new SchedulePaymentCommand(
                eventData.InvoiceId,
                eventData.VendorId,
                eventData.TotalAmount,
                eventData.Currency,
                eventData.DueDate,
                eventData.ApprovedBy);  // Use ApprovedBy as CreatedBy for the payment

            var result = await mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Successfully scheduled payment {PaymentId} for invoice {InvoiceId}",
                    result.Value,
                    eventData.InvoiceId);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to schedule payment for invoice {InvoiceId}: {Error}",
                    eventData.InvoiceId,
                    result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing InvoiceApprovedEvent");
            throw; // Re-throw so the message is NACKed and requeued
        }
    }
}

/// <summary>
/// DTO to deserialize InvoiceApprovedEvent from JSON.
/// </summary>
internal record InvoiceApprovedEventDto(
    Guid InvoiceId,
    Guid VendorId,
    decimal TotalAmount,
    string Currency,
    DateTime DueDate,
    string ApprovedBy);
