using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Application.Messaging;

namespace Shared.Infrastructure.Outbox;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly IEventBus _eventBus;

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessor> logger,
        IEventBus eventBus)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _eventBus = eventBus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContexts = scope.ServiceProvider.GetServices<DbContext>();

                foreach (var context in dbContexts)
                {
                    var messages = await context.Set<OutboxMessage>()
                        .Where(m => m.ProcessedOnUtc == null)
                        .OrderBy(m => m.OccurredOnUtc)
                        .Take(20)
                        .ToListAsync(stoppingToken);

                    foreach (var message in messages)
                    {
                        try
                        {
                            await _eventBus.PublishRawAsync(message.Type, message.Content, stoppingToken);
                            message.ProcessedOnUtc = DateTime.UtcNow;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing outbox message {MessageId}", message.Id);
                            message.Error = ex.Message;
                        }
                    }

                    if (messages.Any())
                    {
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OutboxProcessor");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
