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
        if (_mapping.TryGetValue(handUid, out var events))
        {
            _logger.LogInformation($"{events.Count} events are got for {handUid}");
            return events;
        }

        _logger.LogInformation($"No events are got for {handUid}");
        return [];
    }

    public void AddEvents(HandUid handUid, IList<IEvent> events)
    {
        if (!_mapping.TryAdd(handUid, events.ToList()))
        {
            _mapping[handUid].AddRange(events);
        }
        _logger.LogInformation($"{events.Count} events are added for {handUid}");
    }
}
