using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class BigBlindIsPostedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<BigBlindIsPostedEvent>
{
    public async Task HandleAsync(BigBlindIsPostedEvent @event, EventContext context)
    {
        var integrationEvent = new BlindIsPostedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Nickname = @event.Nickname,
            Amount = @event.Amount,
            OccurredAt = @event.OccurredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.blind-is-posted");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
