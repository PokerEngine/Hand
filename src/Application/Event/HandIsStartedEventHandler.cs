using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class HandIsStartedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HandIsStartedEvent>
{
    public async Task HandleAsync(HandIsStartedEvent @event, EventContext context)
    {
        var integrationEvent = new HandIsStartedIntegrationEvent
        {
            HandUid = context.HandUid,
            OccuredAt = @event.OccuredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.HandType.ToRoutingKey()}.hand-is-started");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
