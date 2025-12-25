using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class DecisionIsCommittedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<DecisionIsCommittedEvent>
{
    public async Task HandleAsync(DecisionIsCommittedEvent @event, HandUid handUid)
    {
        var integrationEvent = new DecisionIsCommittedIntegrationEvent()
        {
            HandUid = handUid,
            Nickname = @event.Nickname,
            DecisionType = @event.Decision.Type.ToString(),
            DecisionAmount = @event.Decision.Amount,
            OccuredAt = @event.OccuredAt
        };

        await integrationEventPublisher.PublishAsync(integrationEvent, IntegrationEventChannel.Hand);
    }
}
