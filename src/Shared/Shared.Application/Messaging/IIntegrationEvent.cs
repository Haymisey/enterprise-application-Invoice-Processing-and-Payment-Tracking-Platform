namespace Shared.Application.Messaging;

/// <summary>
/// Marker interface for integration events.
/// Integration events are domain events that are published to external systems (e.g., RabbitMQ).
/// </summary>
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}