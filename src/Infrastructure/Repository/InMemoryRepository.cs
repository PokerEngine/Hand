using Application.Repository;
using Domain.Event;
using Domain.ValueObject;

namespace Infrastructure.Repository;

public class InMemoryRepository : IRepository
{
    private readonly ILogger<InMemoryRepository> _logger;
    private readonly Dictionary<HandUid, List<BaseEvent>> _mapping = new();

    public InMemoryRepository(ILogger<InMemoryRepository> logger)
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

    public async Task<IList<BaseEvent>> GetEvents(HandUid handUid)
    {
        if (!_mapping.TryGetValue(handUid, out var events))
        {
            events = [];
        }

        _logger.LogInformation("{eventCount} events are got for {handUid}", events.Count, handUid);
        await  Task.CompletedTask;
        return events;
    }

    public async Task AddEvents(HandUid handUid, IList<BaseEvent> events)
    {
        if (!_mapping.TryAdd(handUid, events.ToList()))
        {
            _mapping[handUid].AddRange(events);
        }

        _logger.LogInformation("{eventCount} events are added for {handUid}", events.Count, handUid);
        await  Task.CompletedTask;
    }
}
