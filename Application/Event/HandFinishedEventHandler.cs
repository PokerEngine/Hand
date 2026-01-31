using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class HandFinishedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HandFinishedEvent>
{
    public async Task HandleAsync(HandFinishedEvent @event, EventContext context)
    {
        var integrationEvent = new HandFinishedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString()
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.hand-finished");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
