using Application.Event;
using Application.Repository;
using Domain.Entity;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;

namespace Application.Command;

public record struct StartHandCommand : ICommand
{
    public required Guid Uid { get; init; }
}

public record struct StartHandResponse : ICommandResponse
{
    public required Guid Uid { get; init; }
}

public class StartHandHandler(
    IRepository repository,
    IEventDispatcher eventDispatcher,
    IRandomizer randomizer,
    IEvaluator evaluator
) : ICommandHandler<StartHandCommand, StartHandResponse>
{
    public async Task<StartHandResponse> HandleAsync(StartHandCommand command)
    {
        var hand = Hand.FromEvents(
            uid: command.Uid,
            randomizer: randomizer,
            evaluator: evaluator,
            events: await repository.GetEventsAsync(command.Uid)
        );

        hand.Start();

        var events = hand.PullEvents();
        await repository.AddEventsAsync(hand.Uid, events);

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

        return new StartHandResponse
        {
            Uid = hand.Uid
        };
    }
}
