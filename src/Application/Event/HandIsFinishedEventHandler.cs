using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class HandIsFinishedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HandIsFinishedEvent>
{
    public async Task HandleAsync(HandIsFinishedEvent @event, HandUid handUid)
    {
        var integrationEvent = new HandIsFinishedIntegrationEvent
        {
            HandUid = handUid,
            OccuredAt = @event.OccuredAt
        };

        await integrationEventPublisher.PublishAsync(integrationEvent, IntegrationEventChannel.Hand);
    }
}
