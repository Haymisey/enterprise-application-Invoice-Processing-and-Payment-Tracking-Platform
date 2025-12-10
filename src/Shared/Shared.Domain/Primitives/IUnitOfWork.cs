namespace Shared.Domain.Primitives;

/// <summary>
/// Unit of Work pattern interface.
/// Coordinates changes across multiple repositories and ensures they are persisted atomically.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in this unit of work to the database.
    /// Also dispatches any domain events raised by aggregates.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
