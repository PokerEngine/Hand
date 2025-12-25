using Application.Event;
using Application.Repository;
using Domain.Entity;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Application.Command;

public record struct CommitDecisionCommand : ICommand
{
    public required Guid HandUid { get; init; }
    public required string Nickname { get; init; }
    public required string DecisionType { get; init; }
    public required int DecisionAmount { get; init; }
}

public record struct CommitDecisionResponse : ICommandResponse
{
    public required Guid HandUid { get; init; }
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
            type: (DecisionType)Enum.Parse(typeof(DecisionType), command.DecisionType),
            amount: command.DecisionAmount
        );

        var hand = Hand.FromEvents(
            uid: command.HandUid,
            randomizer: randomizer,
            evaluator: evaluator,
            events: await repository.GetEventsAsync(command.HandUid)
        );

        hand.CommitDecision(command.Nickname, decision);

        var events = hand.PullEvents();
        await repository.AddEventsAsync(hand.Uid, events);

        foreach (var @event in events)
        {
            await eventDispatcher.DispatchAsync(@event, hand.Uid);
        }

        return new CommitDecisionResponse
        {
            HandUid = hand.Uid
        };
    }
}
