using Application.Repository;
using Domain.Entity;
using Domain.Event;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Application.IntegrationEvent;

public class DecisionCommitIntegrationEventHandler : IIntegrationEventHandler<DecisionCommitIntegrationEvent>
{
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly IRepository _repository;
    private readonly IRandomizer _randomizer;
    private readonly IEvaluator _evaluator;

    public DecisionCommitIntegrationEventHandler(
        IIntegrationEventBus integrationEventBus,
        IRepository repository,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        _integrationEventBus = integrationEventBus;
        _repository = repository;
        _randomizer = randomizer;
        _evaluator = evaluator;
    }

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
            randomizer: _randomizer,
            evaluator: _evaluator,
            events: await _repository.GetEvents(handUid)
        );

        var eventBus = new EventBus();
        var events = new List<BaseEvent>();
        var listener = (BaseEvent @event) => events.Add(@event);
        eventBus.Subscribe(listener);

        hand.CommitDecision(
            nickname: nickname,
            decision: decision,
            eventBus: eventBus
        );

        eventBus.Unsubscribe(listener);

        await _repository.AddEvents(handUid, events);

        var publisher = new DomainEventPublisher(
            integrationEventBus: _integrationEventBus,
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
