using Domain.Event;
using Domain.ValueObject;

namespace Application.IntegrationEvent;

public class DomainEventPublisher
{
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly Guid _tableUid;
    private readonly Guid _handUid;

    public DomainEventPublisher(
        IIntegrationEventBus integrationEventBus,
        Guid tableUid,
        Guid handUid
    )
    {
        _integrationEventBus = integrationEventBus;
        _tableUid = tableUid;
        _handUid = handUid;
    }

    public void Publish(IEnumerable<IEvent> events)
    {
        var eventBus = new EventBus();

        eventBus.Subscribe<HandIsCreatedEvent>(HandleHandIsCreated);

        foreach (var @event in events)
        {
            eventBus.Publish(@event);
        }

        eventBus.Unsubscribe<HandIsCreatedEvent>(HandleHandIsCreated);
    }

    private void HandleHandIsCreated(HandIsCreatedEvent @event)
    {
        var queue = new IntegrationEventQueue($"table.{_tableUid}.{_handUid}");
        var integrationEvent = new HandIsCreatedIntegrationEvent(
            Participants: @event.Participants.Select(ProcessParticipant).ToList(),
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private IntegrationEventParticipant ProcessParticipant(Participant participant)
    {
        return new IntegrationEventParticipant(
            Nickname: (string)participant.Nickname,
            Position: participant.Position.ToString(),
            Stake: (int)participant.Stake
        );
    }
}
