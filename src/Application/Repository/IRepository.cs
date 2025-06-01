using Domain.Event;
using Domain.ValueObject;

namespace Application.Repository;

public interface IRepository
{
    public void Connect();
    public void Disconnect();
    public IList<BaseEvent> GetEvents(HandUid handUid);
    public void AddEvents(HandUid handUid, IList<BaseEvent> events);
}
