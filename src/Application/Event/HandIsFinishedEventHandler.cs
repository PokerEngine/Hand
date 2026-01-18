using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class HandIsFinishedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HandIsFinishedEvent>
{
    public async Task HandleAsync(HandIsFinishedEvent @event, EventContext context)
    {
        var integrationEvent = new HandIsFinishedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString()
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.hand-is-finished");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
