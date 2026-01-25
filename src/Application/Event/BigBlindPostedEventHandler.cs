using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class BigBlindPostedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<BigBlindPostedEvent>
{
    public async Task HandleAsync(BigBlindPostedEvent @event, EventContext context)
    {
        var integrationEvent = new BlindPostedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Nickname = @event.Nickname,
            Amount = @event.Amount,
            OccurredAt = @event.OccurredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.blind-posted");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
