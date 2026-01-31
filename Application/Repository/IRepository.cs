using Domain.Event;
using Domain.ValueObject;

namespace Application.Repository;

public interface IRepository
{
    Task<HandUid> GetNextUidAsync();
    Task<List<IEvent>> GetEventsAsync(HandUid handUid);
    Task AddEventsAsync(HandUid handUid, List<IEvent> events);
}
