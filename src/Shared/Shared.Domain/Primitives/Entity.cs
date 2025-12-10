namespace Shared.Domain.Primitives;

/// <summary>
/// Base class for all domain entities with a strongly-typed ID.
/// Entities have identity and lifecycle. Two entities with the same ID are considered equal.
/// </summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : notnull
{
    public TId Id { get; protected init; } = default!;

    protected Entity(TId id)
    {
        Id = id;
    }

    // Required for EF Core
    protected Entity() { }

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> entity && Id.Equals(entity.Id);
    }

    public bool Equals(Entity<TId>? other)
    {
        return other is not null && Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !Equals(left, right);
    }
}
