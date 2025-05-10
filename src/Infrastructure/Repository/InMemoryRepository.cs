using Application.Repository;
using Domain.Event;
using Domain.ValueObject;

namespace Infrastructure.Repository;


public class InMemoryRepository : IRepository
{
    private readonly ILogger<InMemoryRepository> _logger;
    private readonly Dictionary<HandUid, List<IEvent>> _mapping = new();

    public InMemoryRepository(ILogger<InMemoryRepository> logger)
    {
        _logger = logger;
    }

    public void Connect()
    {
        _logger.LogInformation("Connected");
    }

    public void Disconnect()
    {
        _logger.LogInformation("Disconnected");
    }

    public IList<IEvent> GetEvents(HandUid handUid)
    {
        if (!_mapping.TryGetValue(handUid, out var events))
        {
            events = [];
        }

        _logger.LogInformation("{eventCount} events are got for {handUid}", events.Count, handUid);
        return events;
    }

    public void AddEvents(HandUid handUid, IList<IEvent> events)
    {
        _logger.LogInformation("{eventCount} events are added for {handUid}", events.Count, handUid);

        if (!_mapping.TryAdd(handUid, events.ToList()))
        {
            _mapping[handUid].AddRange(events);
        }
    }
}
