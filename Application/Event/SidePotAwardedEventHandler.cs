using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class SidePotAwardedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<SidePotAwardedEvent>
{
    public async Task HandleAsync(SidePotAwardedEvent @event)
    {
        var integrationEvent = new SidePotAwardedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = @event.HandUid,
            TableUid = @event.TableContext.TableUid,
            TableType = @event.TableContext.TableType.ToString(),
            Winners = @event.Winners.Select(n => n.ToString()).ToList(),
            Amount = @event.SidePot.TotalAmount
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{@event.TableContext.TableType.ToRoutingKey()}.side-pot-awarded");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
