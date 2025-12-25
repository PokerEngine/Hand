using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class HandIsStartedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HandIsStartedEvent>
{
    public async Task HandleAsync(HandIsStartedEvent @event, HandUid handUid)
    {
        var integrationEvent = new HandIsStartedIntegrationEvent
        {
            HandUid = handUid,
            OccuredAt = @event.OccuredAt
        };

        await integrationEventPublisher.PublishAsync(integrationEvent, IntegrationEventChannel.Hand);
    }
}
