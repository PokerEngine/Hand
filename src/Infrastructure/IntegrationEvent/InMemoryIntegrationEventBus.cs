using Application.IntegrationEvent;

namespace Infrastructure.IntegrationEvent;

public class InMemoryIntegrationEventBus : IIntegrationEventBus
{
    private readonly ILogger<InMemoryIntegrationEventBus> _logger;
    private readonly Dictionary<IntegrationEventQueue, List<Delegate>> _mapping = new();
    private bool _isConnected;

    public InMemoryIntegrationEventBus(ILogger<InMemoryIntegrationEventBus> logger)
    {
        _logger = logger;
    }

    public async Task Connect()
    {
        if (_isConnected)
        {
            throw new InvalidOperationException("Mongo is already connected");
        }

        _isConnected = true;
        await Task.CompletedTask;

        _logger.LogInformation("Connected");
    }

    public async Task Disconnect()
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("Not connected");
        }

        _isConnected = false;
        await Task.CompletedTask;

        _logger.LogInformation("Disconnected");
    }

    public async Task Subscribe<T>(IIntegrationEventHandler<T> handler, IntegrationEventQueue queue) where T : IIntegrationEvent
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("Not connected");
        }

        _logger.LogInformation("{handler} subscribed to {queue}", handler.GetType().Name, queue);

        if (!_mapping.TryAdd(queue, [handler.Handle]))
        {
            _mapping[queue].Add(handler.Handle);
        }

        await Task.CompletedTask;
    }

    public async Task Unsubscribe<T>(IIntegrationEventHandler<T> handler, IntegrationEventQueue queue) where T : IIntegrationEvent
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("Not connected");
        }

        _logger.LogInformation("{handler} unsubscribed from {queue}", handler.GetType().Name, queue);

        if (_mapping.TryGetValue(queue, out var listeners))
        {
            listeners.Remove(handler.Handle);
        }

        await Task.CompletedTask;
    }

    public async Task Publish<T>(T integrationEvent, IntegrationEventQueue queue) where T : IIntegrationEvent
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("Not connected");
        }

        _logger.LogInformation("{integrationEvent} is published to {queue}", integrationEvent, queue);

        foreach (var (q, listeners) in _mapping)
        {
            if (!q.IsSubQueue(queue))
            {
                continue;
            }

            foreach (var listener in listeners.OfType<Func<T, Task>>())
            {
                _logger.LogInformation("    {typedListener} is called", listener);

                await listener(integrationEvent);
            }
        }

        await Task.CompletedTask;
    }
}
