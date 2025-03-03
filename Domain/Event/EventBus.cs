using Domain.Error;

namespace Domain.Event;

public class EventBus
{
    private IList<Action<BaseEvent>> _listeners;

    public EventBus()
    {
        _listeners = [];
    }

    public void Subscribe(Action<BaseEvent> listener)
    {
        if (_listeners.Contains(listener))
        {
            throw new NotAvailableError("The listener has already been subscribed");
        }

        _listeners.Add(listener);
    }

    public void Unsubscribe(Action<BaseEvent> listener)
    {
        if (!_listeners.Contains(listener))
        {
            throw new NotAvailableError("The listener has not been subscribed yet");
        }

        _listeners.Remove(listener);
    }

    public void Publish(BaseEvent @event)
    {
        foreach (var listener in _listeners)
        {
            listener(@event);
        }
    }
}