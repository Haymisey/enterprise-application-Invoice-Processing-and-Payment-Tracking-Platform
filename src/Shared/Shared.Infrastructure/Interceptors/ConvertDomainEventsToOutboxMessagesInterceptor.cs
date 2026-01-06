using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Domain.Primitives;
using Shared.Infrastructure.Outbox;

namespace Shared.Infrastructure.Interceptors;

/// <summary>
/// EF Core interceptor that automatically converts domain events to OutboxMessage records
/// when SaveChangesAsync is called. This ensures events are saved in the same transaction.
/// </summary>
public sealed class ConvertDomainEventsToOutboxMessagesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;
        if (dbContext is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        // Get all aggregates with domain events
        var aggregates = dbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        // Convert domain events to OutboxMessage records
        var outboxMessages = aggregates
            .SelectMany(aggregate => aggregate.DomainEvents)
            .Select(domainEvent => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOnUtc = domainEvent.OccurredOn,
                Type = domainEvent.GetType().Name,
                Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), new JsonSerializerOptions
                {
                    WriteIndented = false
                })
            })
            .ToList();

        // Clear domain events from aggregates
        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        // Add OutboxMessages to the context
        if (outboxMessages.Any())
        {
            dbContext.Set<OutboxMessage>().AddRange(outboxMessages);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}