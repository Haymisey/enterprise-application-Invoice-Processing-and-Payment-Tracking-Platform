using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reporting.Domain.Entities;
using Reporting.Infrastructure.Persistence;
using Shared.Infrastructure.Messaging;

namespace Reporting.Infrastructure.EventConsumers;

public sealed class ReportingEventConsumer : RabbitMqConsumer
{
    private readonly ILogger<ReportingEventConsumer> _logger;

    protected override string QueueName => "reporting-queue";
    
    protected override string[] SubscribedEventTypes => new[]
    {
        "InvoiceCreatedEvent",
        "InvoiceApprovedEvent",
        "InvoiceRejectedEvent",
        "PaymentCompletedEvent"
    };

    public ReportingEventConsumer(
        IServiceProvider serviceProvider,
        ILogger<ReportingEventConsumer> logger,
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
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();

        switch (eventType)
        {
            case "InvoiceCreatedEvent":
                await HandleInvoiceCreated(eventPayload, context);
                break;
            case "InvoiceApprovedEvent":
                await HandleStatusChange(eventPayload, context, "Draft", "Approved");
                break;
            case "InvoiceRejectedEvent":
                await HandleStatusChange(eventPayload, context, "Approved", "Rejected");
                break;
            case "PaymentCompletedEvent":
                await HandleStatusChange(eventPayload, context, "Approved", "Paid");
                break;
            case "InvoiceFlaggedForReviewEvent":
                await HandleStatusChange(eventPayload, context, "Draft", "FlaggedForReview");
                break;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task HandleInvoiceCreated(string payload, ReportingDbContext context)
    {
        var data = JsonSerializer.Deserialize<InvoiceCreatedDto>(payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (data == null) return;

        var summary = await context.InvoiceSummaries.FindAsync("Draft") 
                      ?? new InvoiceSummary { Status = "Draft" };
        
        summary.Count++;
        summary.TotalAmount += data.TotalAmount;

        if (context.Entry(summary).State == EntityState.Detached)
            context.InvoiceSummaries.Add(summary);
    }

    private async Task HandleStatusChange(string payload, ReportingDbContext context, string oldStatus, string newStatus)
    {
        var data = JsonSerializer.Deserialize<AmountDto>(payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (data == null) return;

        // Use either Amount or TotalAmount
        var amount = data.Amount ?? data.TotalAmount ?? 0;

        var oldSum = await context.InvoiceSummaries.FindAsync(oldStatus);
        if (oldSum != null)
        {
            oldSum.Count--;
            oldSum.TotalAmount -= amount;
        }

        var newSum = await context.InvoiceSummaries.FindAsync(newStatus)
                     ?? new InvoiceSummary { Status = newStatus };
        
        newSum.Count++;
        newSum.TotalAmount += amount;

        if (context.Entry(newSum).State == EntityState.Detached)
            context.InvoiceSummaries.Add(newSum);
    }
}

internal record InvoiceCreatedDto(decimal TotalAmount);
internal record AmountDto(decimal? Amount, decimal? TotalAmount);
