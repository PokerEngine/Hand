using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class HandStartedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HandStartedEvent>
{
    public async Task HandleAsync(HandStartedEvent @event, EventContext context)
    {
        var integrationEvent = new HandStartedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString()
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.hand-started");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
