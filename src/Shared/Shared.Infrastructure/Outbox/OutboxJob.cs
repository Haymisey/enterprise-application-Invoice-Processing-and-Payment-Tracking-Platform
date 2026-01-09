using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Shared.Application.Messaging;

namespace Shared.Infrastructure.Outbox;

[DisallowConcurrentExecution]
public class OutboxJob : IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxJob> _logger;
    private readonly IEventBus _eventBus;

    public OutboxJob(
        IServiceProvider serviceProvider,
        ILogger<OutboxJob> logger,
        IEventBus eventBus)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _eventBus = eventBus;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            // Get all registered DbContexts to process their outboxes
            var dbContexts = scope.ServiceProvider.GetServices<DbContext>();

            foreach (var dbContext in dbContexts)
            {
                var messages = await dbContext.Set<OutboxMessage>()
                    .Where(m => m.ProcessedOnUtc == null)
                    .OrderBy(m => m.OccurredOnUtc)
                    .Take(20)
                    .ToListAsync(context.CancellationToken);

                if (!messages.Any()) continue;

                _logger.LogInformation("Processing {Count} outbox messages for {DbContext}", messages.Count, dbContext.GetType().Name);

                foreach (var message in messages)
                {
                    try
                    {
                        await _eventBus.PublishRawAsync(message.Type, message.Content, context.CancellationToken);
                        message.ProcessedOnUtc = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error publishing outbox message {MessageId}", message.Id);
                        message.Error = ex.Message;
                    }
                }

                await dbContext.SaveChangesAsync(context.CancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in OutboxJob execution");
        }
    }
}
