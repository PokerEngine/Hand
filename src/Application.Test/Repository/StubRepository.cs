using Application.Repository;
using Domain.Event;
using Domain.ValueObject;
using System.Collections.Concurrent;

namespace Application.Test.Repository;

public class StubRepository : IRepository
{
    private readonly ConcurrentDictionary<HandUid, List<IEvent>> _mapping = new();

    public Task<HandUid> GetNextUidAsync()
    {
        return Task.FromResult(new HandUid(Guid.NewGuid()));
    }

    public Task<List<IEvent>> GetEventsAsync(HandUid handUid)
    {
        if (!_mapping.TryGetValue(handUid, out var events))
        {
            throw new InvalidOperationException("The hand is not found");
        }

        List<IEvent> snapshot;
        lock (events)
            snapshot = events.ToList();

        return Task.FromResult(snapshot);
    }

    public Task AddEventsAsync(HandUid handUid, List<IEvent> events)
    {
        var items = _mapping.GetOrAdd(handUid, _ => new List<IEvent>());
        lock (items)
            items.AddRange(events);

        return Task.CompletedTask;
    }
}
