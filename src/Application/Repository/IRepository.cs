using Domain.Event;
using Domain.ValueObject;

namespace Application.Repository;

public interface IRepository
{
    Task<List<IEvent>> GetEvents(HandUid handUid);
    Task AddEvents(HandUid handUid, List<IEvent> events);
}
