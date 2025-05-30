using Application.Repository;
using Domain.Entity;
using Domain.Event;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Application.IntegrationEvent;

public class HandStartIntegrationEventHandler : IIntegrationEventHandler<HandStartIntegrationEvent>
{
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly IRepository _repository;
    private readonly IRandomizer _randomizer;
    private readonly IEvaluator _evaluator;

    public HandStartIntegrationEventHandler(
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

    public void Handle(HandStartIntegrationEvent integrationEvent)
    {
        var handUid = new HandUid(integrationEvent.HandUid);

        var hand = Hand.FromEvents(
            uid: handUid,
            randomizer: _randomizer,
            evaluator: _evaluator,
            events: _repository.GetEvents(handUid)
        );

        var eventBus = new EventBus();
        var events = new List<IEvent>();
        var listener = (IEvent @event) => events.Add(@event);
        eventBus.Subscribe(listener);

        hand.Start(eventBus);

        eventBus.Unsubscribe(listener);

        _repository.AddEvents(handUid, events);

        var publisher = new DomainEventPublisher(
            integrationEventBus: _integrationEventBus,
            tableUid: integrationEvent.TableUid,
            handUid: integrationEvent.HandUid
        );
        publisher.Publish(events);
    }
}
