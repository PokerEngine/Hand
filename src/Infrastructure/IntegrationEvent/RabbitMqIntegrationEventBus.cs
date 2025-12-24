using Application.IntegrationEvent;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Infrastructure.IntegrationEvent;

public class RabbitMqIntegrationEventBus(
    IOptions<RabbitMqIntegrationEventBusOptions> options,
    ILogger<RabbitMqIntegrationEventBus> logger
) : IIntegrationEventBus
{
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly Dictionary<IntegrationEventQueue, Dictionary<Type, string>> _tags = new();
    private bool IsConnected => _channel != null && _channel.IsOpen;

    public async Task Connect()
    {
        if (IsConnected)
        {
            throw new InvalidOperationException("Already connected");
        }

        var factory = new ConnectionFactory
        {
            HostName = options.Value.Host,
            Port = options.Value.Port,
            UserName = options.Value.Username,
            Password = options.Value.Password,
            VirtualHost = options.Value.VirtualHost
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        logger.LogInformation("Connected");
    }

    public async Task Disconnect()
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("Not connected");
        }

        await _channel!.CloseAsync();
        await _connection!.CloseAsync();

        logger.LogInformation("Disconnected");
    }

    public async Task Subscribe<T>(IIntegrationEventHandler<T> handler, IntegrationEventQueue queue) where T : IIntegrationEvent
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("Not connected");
        }

        if (_tags.TryGetValue(queue, out var handlers))
        {
            if (handlers.ContainsKey(handler.GetType()))
            {
                throw new InvalidOperationException($"{handler.GetType().Name} has already subscribed to {queue}");
            }
        }

        var queueName = queue.ToString();

        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonSerializer.Deserialize<T>(json);

                if (message != null)
                {
                    await handler.Handle(message);
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                else
                {
                    logger.LogWarning("Received null deserialized event on queue {queue}", queue);
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling message on queue {queue}", queue);
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        var consumerTag = await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer
        );

        if (_tags.ContainsKey(queue))
        {
            _tags[queue][handler.GetType()] = consumerTag;
        }
        else
        {
            _tags[queue] = new Dictionary<Type, string> { { handler.GetType(), consumerTag } };
        }

        logger.LogInformation("{handler} subscribed to queue {queue}", handler.GetType().Name, queueName);
    }

    public async Task Unsubscribe<T>(IIntegrationEventHandler<T> handler, IntegrationEventQueue queue) where T : IIntegrationEvent
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("Not connected");
        }

        if (_tags.TryGetValue(queue, out var handlers))
        {
            if (handlers.TryGetValue(handler.GetType(), out var tag))
            {
                await _channel.BasicCancelAsync(tag);
                handlers.Remove(handler.GetType());
            }
            else
            {
                throw new InvalidOperationException($"{handler.GetType().Name} has not subscribed to {queue} yet");
            }
        }
        else
        {
            throw new InvalidOperationException($"{handler.GetType().Name} has not subscribed to {queue} yet");
        }

        logger.LogInformation("{handler} unsubscribed from {queue}", handler.GetType().Name, queue);
    }

    public async Task Publish<T>(T integrationEvent, IntegrationEventQueue queue) where T : IIntegrationEvent
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("Not connected");
        }

        var queueName = queue.ToString();

        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var json = JsonSerializer.Serialize(integrationEvent);
        var body = Encoding.UTF8.GetBytes(json);
        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await _channel.BasicPublishAsync(
            exchange: "",
            routingKey: queueName,
            mandatory: false,
            basicProperties: properties,
            body: body
        );

        logger.LogInformation("{integrationEvent} published to queue {queue}", integrationEvent, queue);
    }
}

public class RabbitMqIntegrationEventBusOptions
{
    public const string SectionName = "RabbitMQ";

    public required string Host { get; init; }
    public required int Port { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string VirtualHost { get; init; }
}
