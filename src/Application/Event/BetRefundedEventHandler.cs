using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class BetRefundedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<BetRefundedEvent>
{
    public async Task HandleAsync(BetRefundedEvent @event, EventContext context)
    {
        var integrationEvent = new BetRefundedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Nickname = @event.Nickname,
            Amount = @event.Amount
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.bet-refunded");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
