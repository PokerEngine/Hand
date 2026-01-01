using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class BigBlindIsPostedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<BigBlindIsPostedEvent>
{
    public async Task HandleAsync(BigBlindIsPostedEvent @event, EventContext context)
    {
        var integrationEvent = new BlindIsPostedIntegrationEvent
        {
            HandUid = context.HandUid,
            Nickname = @event.Nickname,
            Amount = @event.Amount,
            OccuredAt = @event.OccuredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.HandType.ToRoutingKey()}.blind-is-posted");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
