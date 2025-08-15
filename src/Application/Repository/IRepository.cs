using Domain.Event;
using Domain.ValueObject;

namespace Application.Repository;

public interface IRepository
{
    Task Connect();
    Task Disconnect();
    Task<IList<BaseEvent>> GetEvents(HandUid handUid);
    Task AddEvents(HandUid handUid, IList<BaseEvent> events);
}
