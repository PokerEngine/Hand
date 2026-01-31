using Application.Event;
using Domain.Event;
using Domain.ValueObject;
using System.Collections.Concurrent;

namespace Application.Test.Event;

public class StubEventDispatcher : IEventDispatcher
{
    private readonly ConcurrentDictionary<HandUid, List<IEvent>> _mapping = new();

    public Task DispatchAsync(IEvent @event, EventContext context)
    {
        var items = _mapping.GetOrAdd(context.HandUid, _ => new List<IEvent>());
        lock (items)
            items.Add(@event);

        return Task.CompletedTask;
    }

    public Task<List<IEvent>> GetDispatchedEventsAsync(HandUid handUid)
    {
        if (!_mapping.TryGetValue(handUid, out var events))
        {
            return Task.FromResult(new List<IEvent>());
        }

        List<IEvent> snapshot;
        lock (events)
            snapshot = events.ToList();

        return Task.FromResult(snapshot);
    }

    public Task ClearDispatchedEventsAsync(HandUid handUid)
    {
        if (_mapping.TryGetValue(handUid, out var items))
        {
            lock (items)
            {
                items.Clear();
            }
        }

        return Task.CompletedTask;
    }
}
