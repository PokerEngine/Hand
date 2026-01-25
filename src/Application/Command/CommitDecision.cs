using Application.Event;
using Application.Repository;
using Domain.Entity;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Application.Command;

public record CommitDecisionCommand : ICommand
{
    public required Guid Uid { get; init; }
    public required string Nickname { get; init; }
    public required string Type { get; init; }
    public required int Amount { get; init; }
}

public record CommitDecisionResponse : ICommandResponse
{
    public required Guid Uid { get; init; }
    public required string Nickname { get; init; }
}

public class CommitDecisionHandler(
    IRepository repository,
    IEventDispatcher eventDispatcher,
    IRandomizer randomizer,
    IEvaluator evaluator
) : ICommandHandler<CommitDecisionCommand, CommitDecisionResponse>
{
    public async Task<CommitDecisionResponse> HandleAsync(CommitDecisionCommand command)
    {
        var decision = new Decision(
            type: (DecisionType)Enum.Parse(typeof(DecisionType), command.Type),
            amount: command.Amount
        );

        var hand = Hand.FromEvents(
            uid: command.Uid,
            randomizer: randomizer,
            evaluator: evaluator,
            events: await repository.GetEventsAsync(command.Uid)
        );

        hand.CommitDecision(command.Nickname, decision);

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

        return new CommitDecisionResponse
        {
            Uid = hand.Uid,
            Nickname = command.Nickname,
        };
    }
}
