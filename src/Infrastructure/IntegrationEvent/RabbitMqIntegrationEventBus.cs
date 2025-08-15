using Application.IntegrationEvent;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Infrastructure.IntegrationEvent;

public class RabbitMqIntegrationEventBus : IIntegrationEventBus
{
    private readonly ILogger<RabbitMqIntegrationEventBus> _logger;
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _virtualHost;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly Dictionary<IntegrationEventQueue, Dictionary<Type, string>> _tags = new();
    private bool IsConnected => _channel != null && _channel.IsOpen;

    public RabbitMqIntegrationEventBus(IConfiguration configuration, ILogger<RabbitMqIntegrationEventBus> logger)
    {
        _logger = logger;

        _host = configuration.GetValue<string>("RabbitMQ:Host") ??
                   throw new ArgumentException("RabbitMQ:Host is not configured", nameof(configuration));
        _port = configuration.GetValue<int?>("RabbitMQ:Port") ??
                throw new ArgumentException("RabbitMQ:Port is not configured", nameof(configuration));
        _username = configuration.GetValue<string>("RabbitMQ:Username") ??
                    throw new ArgumentException("RabbitMQ:Username is not configured", nameof(configuration));
        _password = configuration.GetValue<string>("RabbitMQ:Password") ??
                    throw new ArgumentException("RabbitMQ:Password is not configured", nameof(configuration));
        _virtualHost = configuration.GetValue<string>("RabbitMQ:VirtualHost") ??
                       throw new ArgumentException("RabbitMQ:VirtualHost is not configured", nameof(configuration));
    }

    public async Task Connect()
    {
        if (IsConnected)
        {
            throw new InvalidOperationException("Already connected");
        }

        var factory = new ConnectionFactory
        {
            HostName = _host,
            Port = _port,
            UserName = _username,
            Password = _password,
            VirtualHost = _virtualHost
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        _logger.LogInformation("Connected");
    }

    public async Task Disconnect()
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("Not connected");
        }

        await _channel.CloseAsync();
        await _connection.CloseAsync();

        _logger.LogInformation("Disconnected");
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
                    _logger.LogWarning("Received null deserialized event on queue {queue}", queue);
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling message on queue {queue}", queue);
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

        _logger.LogInformation("{handler} subscribed to queue {queue}", handler.GetType().Name, queueName);
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

        _logger.LogInformation("{handler} unsubscribed from {queue}", handler.GetType().Name, queue);
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

        _logger.LogInformation("{integrationEvent} published to queue {queue}", integrationEvent, queue);
    }
}
