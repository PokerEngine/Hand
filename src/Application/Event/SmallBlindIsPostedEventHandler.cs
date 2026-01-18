using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class SmallBlindIsPostedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<SmallBlindIsPostedEvent>
{
    public async Task HandleAsync(SmallBlindIsPostedEvent @event, EventContext context)
    {
        var integrationEvent = new BlindIsPostedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Nickname = @event.Nickname,
            Amount = @event.Amount
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.blind-is-posted");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
