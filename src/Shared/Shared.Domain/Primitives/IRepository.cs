namespace Shared.Domain.Primitives;

/// <summary>
/// Generic repository interface following the Repository pattern.
/// Repositories provide collection-like access to aggregates and abstract persistence details.
/// </summary>
public interface IRepository<TAggregate, TId> 
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    /// <summary>
    /// Gets an aggregate by its ID.
    /// </summary>
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new aggregate to the repository.
    /// </summary>
    Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing aggregate.
    /// </summary>
    void Update(TAggregate aggregate);

    /// <summary>
    /// Removes an aggregate from the repository.
    /// </summary>
    void Remove(TAggregate aggregate);
}
