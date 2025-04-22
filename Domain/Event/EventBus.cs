using Domain.Error;

namespace Domain.Event;

public interface IEventBus
{
    public void Subscribe<T>(Action<T> listener) where T : IEvent;
    public void Unsubscribe<T>(Action<T> listener) where T : IEvent;
    public void Publish<T>(T @event) where T : IEvent;
}

public class EventBus : IEventBus
{
    private readonly List<Delegate> _listeners = [];

    public void Subscribe<T>(Action<T> listener) where T : IEvent
    {
        _listeners.Add(listener);
    }

    public void Unsubscribe<T>(Action<T> listener) where T : IEvent
    {
        _listeners.Remove(listener);
    }

    public void Publish<T>(T @event) where T : IEvent
    {
        foreach (var listener in _listeners)
        {
            if (listener is Action<T> typedListener)
            {
                typedListener(@event);
            }
        }
    }
}
