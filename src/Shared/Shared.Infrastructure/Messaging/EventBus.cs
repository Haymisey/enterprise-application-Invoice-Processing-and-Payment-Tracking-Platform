using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Shared.Application.Messaging;

namespace Shared.Infrastructure.Messaging;

public class EventBus : IEventBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string ExchangeName = "invoice-platform-exchange";

    public EventBus(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:UserName"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, durable: true);
    }

    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        var message = JsonSerializer.Serialize(integrationEvent);
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: string.Empty,
            basicProperties: null,
            body: body);

        return Task.CompletedTask;
    }

    public Task PublishRawAsync(string type, string content, CancellationToken cancellationToken = default)
    {
        var body = Encoding.UTF8.GetBytes(content);

        var properties = _channel.CreateBasicProperties();
        properties.Type = type;

        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: string.Empty,
            basicProperties: properties,
            body: body);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
