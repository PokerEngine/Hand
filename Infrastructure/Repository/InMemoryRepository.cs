using Domain.Event;
using Domain.ValueObject;
using Application;

namespace Infrastructure.Repository;


public class InMemoryRepository : IRepository
{
    private readonly Dictionary<HandUid, List<IEvent>> _mapping = new ();

    public void Connect()
    {
    }

    public void Disconnect()
    {
    }

    public IList<IEvent> GetEvents(HandUid handUid)
    {
        if (_mapping.TryGetValue(handUid, out var events))
        {
            return events.ToList();
        }

        return [];
    }

    public void AddEvents(HandUid handUid, IList<IEvent> events)
    {
        if (!_mapping.TryAdd(handUid, events.ToList()))
        {
            _mapping[handUid].AddRange(events);
        }
    }
}