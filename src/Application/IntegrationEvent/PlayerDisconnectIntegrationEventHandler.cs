using Application.Repository;
using Domain.Entity;
using Domain.Event;
using Domain.ValueObject;

namespace Application.IntegrationEvent;

public class PlayerDisconnectIntegrationEventHandler : IIntegrationEventHandler<PlayerDisconnectIntegrationEvent>
{
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly IRepository _repository;

    public PlayerDisconnectIntegrationEventHandler(
        IIntegrationEventBus integrationEventBus,
        IRepository repository
    )
    {
        _integrationEventBus = integrationEventBus;
        _repository = repository;
    }

    public void Handle(PlayerDisconnectIntegrationEvent integrationEvent)
    {
        var handUid = new HandUid(integrationEvent.HandUid);
        var nickname = new Nickname(integrationEvent.Nickname);

        var hand = Hand.FromEvents(
            uid: handUid,
            events: _repository.GetEvents(handUid)
        );

        var eventBus = new EventBus();
        var events = new List<IEvent>();
        var listener = (IEvent @event) => events.Add(@event);
        eventBus.Subscribe(listener);

        hand.DisconnectPlayer(
            nickname: nickname,
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
}
