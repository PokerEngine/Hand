using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class RefundIsCommittedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<RefundIsCommittedEvent>
{
    public async Task HandleAsync(RefundIsCommittedEvent @event, EventContext context)
    {
        var integrationEvent = new RefundIsCommittedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Nickname = @event.Nickname,
            Amount = @event.Amount
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.refund-is-committed");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
