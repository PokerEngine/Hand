using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class HandFinishedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HandFinishedEvent>
{
    public async Task HandleAsync(HandFinishedEvent @event)
    {
        var integrationEvent = new HandFinishedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = @event.HandUid,
            TableUid = @event.TableContext.TableUid,
            TableType = @event.TableContext.TableType.ToString()
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{@event.TableContext.TableType.ToRoutingKey()}.hand-finished");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
