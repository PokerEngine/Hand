using Application.IntegrationEvent;

namespace Infrastructure.IntegrationEvent;

public class InMemoryIntegrationEventBus : IIntegrationEventBus
{
    private readonly Dictionary<IntegrationEventQueue, List<Delegate>> _mapping = new ();

    public void Connect()
    {
    }

    public void Disconnect()
    {
    }

    public void Subscribe<T>(IIntegrationEventHandler<T> handler, IntegrationEventQueue queue) where T : IIntegrationEvent
    {
        if (!_mapping.TryAdd(queue, [handler.Handle]))
        {
            _mapping[queue].Add(handler.Handle);
        }
    }

    public void Unsubscribe<T>(IIntegrationEventHandler<T> handler, IntegrationEventQueue queue) where T : IIntegrationEvent
    {
        if (_mapping.TryGetValue(queue, out var listeners))
        {
            listeners.Remove(handler.Handle);
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
    }
}