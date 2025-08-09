using Application.Repository;
using Domain.Event;
using Domain.ValueObject;

namespace Infrastructure.Repository;

public class InMemoryRepository : IRepository
{
    private readonly ILogger<InMemoryRepository> _logger;
    private readonly Dictionary<HandUid, List<BaseEvent>> _mapping = new();
    private bool _isConnected;

    public InMemoryRepository(ILogger<InMemoryRepository> logger)
    {
        _logger = logger;
    }

    public async Task Connect()
    {
        if (_isConnected)
        {
            throw new InvalidOperationException("Already connected");
        }

        _logger.LogInformation("Connected");
        _isConnected = true;
        await Task.CompletedTask;
    }

    public async Task Disconnect()
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("Not connected");
        }

        _logger.LogInformation("Disconnected");
        _isConnected = false;
        await Task.CompletedTask;
    }

    public async Task<IList<BaseEvent>> GetEvents(HandUid handUid)
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("Not connected");
        }

        if (!_mapping.TryGetValue(handUid, out var events))
        {
            events = [];
        }

        _logger.LogInformation("{eventCount} events are got for {handUid}", events.Count, handUid);
        await Task.CompletedTask;
        return events;
    }

    public async Task AddEvents(HandUid handUid, IList<BaseEvent> events)
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("Not connected");
        }

        if (!_mapping.TryAdd(handUid, events.ToList()))
        {
            _mapping[handUid].AddRange(events);
        }

        _logger.LogInformation("{eventCount} events are added for {handUid}", events.Count, handUid);
        await Task.CompletedTask;
    }
}
