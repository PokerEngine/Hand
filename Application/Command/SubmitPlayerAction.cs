using Application.Event;
using Application.Repository;
using Domain.Entity;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Application.Command;

public record SubmitPlayerActionCommand : ICommand
{
    public required Guid Uid { get; init; }
    public required string Nickname { get; init; }
    public required string Type { get; init; }
    public required int Amount { get; init; }
}

public record SubmitPlayerActionResponse : ICommandResponse
{
    public required Guid Uid { get; init; }
    public required string Nickname { get; init; }
}

public class SubmitPlayerActionHandler(
    IRepository repository,
    IEventDispatcher eventDispatcher,
    IRandomizer randomizer,
    IEvaluator evaluator
) : ICommandHandler<SubmitPlayerActionCommand, SubmitPlayerActionResponse>
{
    public async Task<SubmitPlayerActionResponse> HandleAsync(SubmitPlayerActionCommand command)
    {
        var action = new PlayerAction(
            type: (PlayerActionType)Enum.Parse(typeof(PlayerActionType), command.Type),
            amount: command.Amount
        );

        var hand = Hand.FromEvents(
            uid: command.Uid,
            randomizer: randomizer,
            evaluator: evaluator,
            events: await repository.GetEventsAsync(command.Uid)
        );

        hand.SubmitPlayerAction(command.Nickname, action);

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

        return new SubmitPlayerActionResponse
        {
            Uid = hand.Uid,
            Nickname = command.Nickname,
        };
    }
}
