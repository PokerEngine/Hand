using Application.Repository;
using Domain.Entity;
using Domain.Error;
using Domain.Event;
using Domain.ValueObject;

namespace Application.IntegrationEvent;

public class DecisionCommitIntegrationEventHandler : IIntegrationEventHandler<DecisionCommitIntegrationEvent>
{
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly IRepository _repository;

    public DecisionCommitIntegrationEventHandler(
        IIntegrationEventBus integrationEventBus,
        IRepository repository
    )
    {
        _integrationEventBus = integrationEventBus;
        _repository = repository;
    }

    public void Handle(DecisionCommitIntegrationEvent integrationEvent)
    {
        var handUid = new HandUid(integrationEvent.HandUid);
        var nickname = new Nickname(integrationEvent.Nickname);
        var decision = new Decision(
            type: ParseDecisionType(integrationEvent.DecisionType),
            amount: new Chips(integrationEvent.DecisionAmount)
        );

        var hand = Hand.FromEvents(
            uid: handUid,
            events: _repository.GetEvents(handUid)
        );

        var eventBus = new EventBus();
        var events = new List<IEvent>();
        var listener = (IEvent @event) => events.Add(@event);
        eventBus.Subscribe(listener);

        hand.CommitDecision(
            nickname: nickname,
            decision: decision,
            eventBus: eventBus
        );

        eventBus.Unsubscribe(listener);

        _repository.AddEvents(handUid, events);

        var publisher = new DomainEventPublisher(
            integrationEventBus: _integrationEventBus,
            tableUid: integrationEvent.TableUid,
            handUid: integrationEvent.HandUid
        );
        publisher.Publish(events);
    }

    private DecisionType ParseDecisionType(string value)
    {
        if (Enum.TryParse(value, out DecisionType decisionType))
        {
            return decisionType;
        }

        throw new NotValidError($"Invalid decision type: {value}");
    }
}
