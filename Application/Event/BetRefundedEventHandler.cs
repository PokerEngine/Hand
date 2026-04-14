using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class BetRefundedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<BetRefundedEvent>
{
    public async Task HandleAsync(BetRefundedEvent @event)
    {
        var integrationEvent = new BetRefundedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = @event.HandUid,
            TableUid = @event.TableContext.TableUid,
            TableType = @event.TableContext.TableType.ToString(),
            Nickname = @event.Nickname,
            Amount = @event.Amount
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{@event.TableContext.TableType.ToRoutingKey()}.bet-refunded");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
