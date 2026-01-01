using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class HandIsFinishedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HandIsFinishedEvent>
{
    public async Task HandleAsync(HandIsFinishedEvent @event, EventContext context)
    {
        var integrationEvent = new HandIsFinishedIntegrationEvent
        {
            HandUid = context.HandUid,
            OccuredAt = @event.OccuredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.HandType.ToRoutingKey()}.hand-is-finished");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
