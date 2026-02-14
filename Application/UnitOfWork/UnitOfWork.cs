using Application.Event;
using Application.Repository;
using Application.Storage;
using Domain.Entity;

namespace Application.UnitOfWork;

public class UnitOfWork(
    IRepository repository,
    IStorage storage,
    IEventDispatcher eventDispatcher
) : IUnitOfWork
{
    private readonly HashSet<Hand> _hands = [];

    public void RegisterHand(Hand hand)
    {
        _hands.Add(hand);
    }

    public async Task CommitAsync(bool updateViews = true)
    {
        foreach (var hand in _hands)
        {
            var events = hand.PullEvents();

            if (events.Count == 0)
            {
                continue;
            }

            await repository.AddEventsAsync(hand.Uid, events);

            if (updateViews)
            {
                await storage.SaveViewAsync(hand);
            }

            var context = new EventContext
            {
                HandUid = hand.Uid,
                TableUid = hand.TableUid,
                TableType = hand.TableType
            };

            foreach (var @event in events)
            {
                await eventDispatcher.DispatchAsync(@event, context);
            }
        }

        _hands.Clear();
    }
}
