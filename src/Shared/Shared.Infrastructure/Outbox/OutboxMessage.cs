namespace Shared.Infrastructure.Outbox;

/// <summary>
/// Represents a domain event that needs to be published to the message broker.
/// This is part of the Transactional Outbox Pattern to ensure reliable event publishing.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime OccurredOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
}