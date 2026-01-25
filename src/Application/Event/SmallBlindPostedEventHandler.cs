using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class SmallBlindPostedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<SmallBlindPostedEvent>
{
    public async Task HandleAsync(SmallBlindPostedEvent @event, EventContext context)
    {
        var integrationEvent = new BlindPostedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Nickname = @event.Nickname,
            Amount = @event.Amount
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.blind-posted");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
