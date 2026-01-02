using Application.IntegrationEvent;
using Domain.Event;

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
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            OccurredAt = @event.OccurredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.hand-is-started");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
