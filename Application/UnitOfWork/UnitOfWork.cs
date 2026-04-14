using Application.Event;
using Application.Repository;
using Application.Storage;
using Domain.Entity;
using Domain.Event;

namespace Application.UnitOfWork;

public class UnitOfWork(
    IRepository repository,
    IStorage storage,
    IEventDispatcher eventDispatcher
) : IUnitOfWork
{
    private readonly List<Func<Task>> _commits = [];

    public void RegisterHand(Hand hand) =>
        _commits.Add(() => CommitAsync(
            hand,
            events => repository.AddEventsAsync(hand.Uid, events)
        ));

    public async Task CommitAsync()
    {
        foreach (var commit in _commits)
            await commit();
        _commits.Clear();
    }

    private async Task CommitAsync(Hand hand, Func<List<IEvent>, Task> persist)
    {
        var events = hand.PullEvents();
        if (events.Count == 0) return;
        await persist(events);
        await storage.SaveViewAsync(hand);
        foreach (var @event in events)
            await eventDispatcher.DispatchAsync(@event);
    }
}
