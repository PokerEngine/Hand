using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class SmallBlindPostedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<SmallBlindPostedEvent>
{
    public async Task HandleAsync(SmallBlindPostedEvent @event)
    {
        var integrationEvent = new BlindPostedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = @event.HandUid,
            TableUid = @event.TableContext.TableUid,
            TableType = @event.TableContext.TableType.ToString(),
            Nickname = @event.Nickname,
            Amount = @event.Amount
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{@event.TableContext.TableType.ToRoutingKey()}.blind-posted");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
