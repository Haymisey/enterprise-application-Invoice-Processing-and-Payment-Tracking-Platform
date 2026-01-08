using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shared.Infrastructure.Messaging;

/// <summary>
/// Base class for RabbitMQ event consumers.
/// Each module can inherit from this to subscribe to specific event types.
/// </summary>
public abstract class RabbitMqConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;
    private const string ExchangeName = "invoice-platform-exchange";

    protected abstract string QueueName { get; }
    protected abstract string[] SubscribedEventTypes { get; }

    protected RabbitMqConsumer(
        IServiceProvider serviceProvider,
        ILogger logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting RabbitMQ consumer for queue: {QueueName}", QueueName);

        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest"
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, durable: true);
            _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(QueueName, ExchangeName, string.Empty);

            _logger.LogInformation("RabbitMQ consumer connected and queue {QueueName} bound to exchange", QueueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ");
        }

        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel is null)
        {
            _logger.LogWarning("RabbitMQ channel not initialized, consumer not started");
            return Task.CompletedTask;
        }

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var eventType = ea.BasicProperties?.Type ?? "Unknown";
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            _logger.LogInformation("Received event: {EventType}", eventType);

            // Only process events we're subscribed to
            if (SubscribedEventTypes.Contains(eventType) || SubscribedEventTypes.Contains("*"))
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    await HandleEventAsync(eventType, message, scope.ServiceProvider, stoppingToken);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling event {EventType}", eventType);
                    // Negative acknowledge - will be requeued
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            }
            else
            {
                // Acknowledge events we don't care about
                _channel.BasicAck(ea.DeliveryTag, false);
            }
        };

        _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Override this method to handle specific event types.
    /// </summary>
    protected abstract Task HandleEventAsync(
        string eventType, 
        string eventPayload, 
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping RabbitMQ consumer for queue: {QueueName}", QueueName);
        _channel?.Close();
        _connection?.Close();
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
