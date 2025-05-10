using Application.IntegrationEvent;

namespace Infrastructure.IntegrationEvent;

public class InMemoryIntegrationEventBus : IIntegrationEventBus
{
    private readonly ILogger<InMemoryIntegrationEventBus> _logger;
    private readonly Dictionary<IntegrationEventQueue, List<Delegate>> _mapping = new();

    public InMemoryIntegrationEventBus(ILogger<InMemoryIntegrationEventBus> logger)
    {
        _logger = logger;
    }

    public void Connect()
    {
        _logger.LogInformation("Connected");
    }

    public void Disconnect()
    {
        _logger.LogInformation("Disconnected");
    }

    public void Subscribe<T>(IIntegrationEventHandler<T> handler, IntegrationEventQueue queue) where T : IIntegrationEvent
    {
        if (!_mapping.TryAdd(queue, [handler.Handle]))
        {
            _mapping[queue].Add(handler.Handle);
            _logger.LogInformation($"{handler.GetType()} subscribed to {queue}");
        }
    }

    public void Unsubscribe<T>(IIntegrationEventHandler<T> handler, IntegrationEventQueue queue) where T : IIntegrationEvent
    {
        if (_mapping.TryGetValue(queue, out var listeners))
        {
            listeners.Remove(handler.Handle);
            _logger.LogInformation($"{handler.GetType()} unsubscribed from {queue}");
        }
    }

    public void Publish<T>(T integrationEvent, IntegrationEventQueue queue) where T : IIntegrationEvent
    {
        foreach (var (q, listeners) in _mapping)
        {
            if (!q.IsSubQueue(queue))
            {
                continue;
            }

            foreach (var listener in listeners)
            {
                if (listener is Action<T> typedListener)
                {
                    typedListener(integrationEvent);
                }
            }
        }

        _logger.LogInformation($"{integrationEvent} is published to {queue}");
    }
}
