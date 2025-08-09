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

    public async Task Connect()
    {
        _logger.LogInformation("Connected");
        await Task.CompletedTask;
    }

    public async Task Disconnect()
    {
        _logger.LogInformation("Disconnected");
        await Task.CompletedTask;
    }

    public async Task Subscribe<T>(IIntegrationEventHandler<T> handler, IntegrationEventQueue queue) where T : IIntegrationEvent
    {
        _logger.LogInformation("{handler} subscribed to {queue}", handler.GetType().Name, queue);

        if (!_mapping.TryAdd(queue, [handler.Handle]))
        {
            _mapping[queue].Add(handler.Handle);
        }

        await Task.CompletedTask;
    }

    public async Task Unsubscribe<T>(IIntegrationEventHandler<T> handler, IntegrationEventQueue queue) where T : IIntegrationEvent
    {
        _logger.LogInformation("{handler} unsubscribed from {queue}", handler.GetType().Name, queue);

        if (_mapping.TryGetValue(queue, out var listeners))
        {
            listeners.Remove(handler.Handle);
        }

        await Task.CompletedTask;
    }

    public async Task Publish<T>(T integrationEvent, IntegrationEventQueue queue) where T : IIntegrationEvent
    {
        _logger.LogInformation("{integrationEvent} is published to {queue}", integrationEvent, queue);

        foreach (var (q, listeners) in _mapping)
        {
            if (!q.IsSubQueue(queue))
            {
                continue;
            }

            foreach (var listener in listeners)
            {
                if (listener is Func<T, Task> typedListener)
                {
                    _logger.LogInformation("    {typedListener} is called", typedListener);

                    await typedListener(integrationEvent);
                }
            }
        }

        await Task.CompletedTask;
    }
}
