
namespace Domain.Event;

public interface IEventBus
{
    public void Subscribe<T>(Action<T> listener) where T : BaseEvent;
    public void Unsubscribe<T>(Action<T> listener) where T : BaseEvent;
    public void Publish<T>(T @event) where T : BaseEvent;
}

public class EventBus : IEventBus
{
    private readonly List<Delegate> _listeners = [];

    public void Subscribe<T>(Action<T> listener) where T : BaseEvent
    {
        _listeners.Add(listener);
    }

    public void Unsubscribe<T>(Action<T> listener) where T : BaseEvent
    {
        _listeners.Remove(listener);
    }

    public void Publish<T>(T @event) where T : BaseEvent
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
