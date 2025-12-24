using Application.Repository;
using Domain.Event;
using Domain.ValueObject;

namespace Infrastructure.Repository;

public class InMemoryRepository(ILogger<InMemoryRepository> logger) : IRepository
{
    private readonly Dictionary<HandUid, List<IEvent>> _mapping = new();

    public async Task<List<IEvent>> GetEvents(HandUid handUid)
    {
        if (!_mapping.TryGetValue(handUid, out var events))
        {
            events = [];
        }

        await Task.CompletedTask;

        logger.LogInformation("{Count} events are got for {HandUid}", events.Count, handUid);
        return events;
    }

    public async Task AddEvents(HandUid handUid, List<IEvent> events)
    {
        if (!_mapping.TryAdd(handUid, events.ToList()))
        {
            _mapping[handUid].AddRange(events);
        }

        await Task.CompletedTask;

        logger.LogInformation("{Count} events are added for {HandUid}", events.Count, handUid);
    }
}
