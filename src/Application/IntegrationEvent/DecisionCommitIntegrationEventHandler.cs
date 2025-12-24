using Application.Repository;
using Domain.Entity;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Application.IntegrationEvent;

public class DecisionCommitIntegrationEventHandler(
    IIntegrationEventBus integrationEventBus,
    IRepository repository,
    IRandomizer randomizer,
    IEvaluator evaluator
) : IIntegrationEventHandler<DecisionCommitIntegrationEvent>
{
    public async Task Handle(DecisionCommitIntegrationEvent integrationEvent)
    {
        var handUid = new HandUid(integrationEvent.HandUid);
        var nickname = new Nickname(integrationEvent.Nickname);
        var decision = new Decision(
            type: ParseDecisionType(integrationEvent.DecisionType),
            amount: new Chips(integrationEvent.DecisionAmount)
        );

        var hand = Hand.FromEvents(
            uid: handUid,
            randomizer: randomizer,
            evaluator: evaluator,
            events: await repository.GetEvents(handUid)
        );

        hand.CommitDecision(nickname, decision);

        var events = hand.PullEvents();
        await repository.AddEvents(handUid, events);

        var publisher = new DomainEventPublisher(
            integrationEventBus: integrationEventBus,
            tableUid: integrationEvent.TableUid,
            handUid: integrationEvent.HandUid
        );
        await publisher.Publish(events);
    }

    private DecisionType ParseDecisionType(string value)
    {
        if (Enum.TryParse(value, out DecisionType decisionType))
        {
            return decisionType;
        }

        throw new ArgumentException("Invalid decision type", nameof(value));
    }
}
