using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class BigBlindPostedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<BigBlindPostedEvent>
{
    public async Task HandleAsync(BigBlindPostedEvent @event)
    {
        var integrationEvent = new BlindPostedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            HandUid = @event.HandUid,
            TableUid = @event.TableContext.TableUid,
            TableType = @event.TableContext.TableType.ToString(),
            Nickname = @event.Nickname,
            Amount = @event.Amount,
            OccurredAt = @event.OccurredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{@event.TableContext.TableType.ToRoutingKey()}.blind-posted");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
