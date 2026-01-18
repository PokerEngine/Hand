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
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString()
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.hand-is-started");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
