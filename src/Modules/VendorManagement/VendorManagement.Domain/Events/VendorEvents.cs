using Shared.Domain.Primitives;

namespace VendorManagement.Domain.Events;

/// <summary>
/// Raised when a new vendor is registered.
/// </summary>
public sealed record VendorRegisteredEvent(
    Guid VendorId,
    string Name,
    string Email) : DomainEvent;

/// <summary>
/// Raised when a vendor is approved/activated.
/// </summary>
public sealed record VendorActivatedEvent(
    Guid VendorId,
    string ActivatedBy) : DomainEvent;

/// <summary>
/// Raised when a vendor is suspended.
/// </summary>
public sealed record VendorSuspendedEvent(
    Guid VendorId,
    string Reason,
    string SuspendedBy) : DomainEvent;

/// <summary>
/// Raised when vendor information is updated.
/// </summary>
public sealed record VendorUpdatedEvent(
    Guid VendorId,
    string Name) : DomainEvent;
