namespace Shared.Domain.Primitives;

/// <summary>
/// Marker interface for domain events.
/// Domain events signal that something important has occurred in the domain.
/// They are published after aggregates are successfully persisted.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for this specific event occurrence.
    /// </summary>
    Guid EventId { get; }
    
    /// <summary>
    /// When the event occurred.
    /// </summary>
    DateTime OccurredOn { get; }
}

/// <summary>
/// Base implementation of a domain event.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
