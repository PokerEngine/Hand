using Application.Repository;
using Domain.Event;
using Domain.ValueObject;
using System.Collections.Concurrent;

namespace Infrastructure.Repository;

public class InMemoryRepository(ILogger<InMemoryRepository> logger) : IRepository
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

        logger.LogInformation("{Count} events are got for {HandUid}", snapshot.Count, handUid);
        return Task.FromResult(snapshot);
    }

    public Task AddEventsAsync(HandUid handUid, List<IEvent> events)
    {
        var items = _mapping.GetOrAdd(handUid, _ => new List<IEvent>());
        lock (items)
            items.AddRange(events);

        logger.LogInformation("{Count} events are added for {HandUid}", events.Count, handUid);
        return Task.CompletedTask;
    }
}
