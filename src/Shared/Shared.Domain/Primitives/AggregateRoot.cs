namespace Shared.Domain.Primitives;

/// <summary>
/// Base class for Aggregate Roots.
/// An Aggregate is a cluster of domain objects that are treated as a single unit.
/// The Aggregate Root is the only entry point to the Aggregate and enforces all invariants.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot(TId id) : base(id)
    {
    }

    // Required for EF Core
    protected AggregateRoot() : base()
    {
    }

    /// <summary>
    /// Raises a domain event to be published after the aggregate is persisted.
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events. Called after events are dispatched.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
