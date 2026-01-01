using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class RefundIsCommittedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<RefundIsCommittedEvent>
{
    public async Task HandleAsync(RefundIsCommittedEvent @event, EventContext context)
    {
        var integrationEvent = new RefundIsCommittedIntegrationEvent
        {
            HandUid = context.HandUid,
            Nickname = @event.Nickname,
            Amount = @event.Amount,
            OccuredAt = @event.OccuredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.HandType.ToRoutingKey()}.refund-is-committed");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
