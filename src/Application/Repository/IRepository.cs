using Domain.Event;
using Domain.ValueObject;

namespace Application.Repository;

public interface IRepository
{
    public Task Connect();
    public Task Disconnect();
    public Task<IList<BaseEvent>> GetEvents(HandUid handUid);
    public Task AddEvents(HandUid handUid, IList<BaseEvent> events);
}
