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
    private List<Delegate> Listeners = [];

    public void Subscribe<T>(Action<T> listener) where T : IEvent
    {
        if (Listeners.Contains(listener))
        {
            throw new NotAvailableError("The listener has already been subscribed");
        }

        Listeners.Add(listener);
    }

    public void Unsubscribe<T>(Action<T> listener) where T : IEvent
    {
        if (!Listeners.Contains(listener))
        {
            throw new NotAvailableError("The listener has not been subscribed yet");
        }

        Listeners.Remove(listener);
    }

    public void Publish<T>(T @event) where T : IEvent
    {
        foreach (var listener in Listeners)
        {
            if (listener is Action<T> typedListener)
            {
                typedListener(@event);
            }
        }
    }
}