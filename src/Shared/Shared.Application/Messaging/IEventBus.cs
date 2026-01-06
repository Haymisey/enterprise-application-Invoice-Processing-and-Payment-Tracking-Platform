using Shared.Application.Exceptions;

namespace Shared.Application.Messaging;

public interface IEventBus
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent;

    Task PublishRawAsync(string type, string content, CancellationToken cancellationToken = default);
}
