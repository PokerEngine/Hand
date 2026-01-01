using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class HoleCardsAreMuckedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HoleCardsAreMuckedEvent>
{
    public async Task HandleAsync(HoleCardsAreMuckedEvent @event, EventContext context)
    {
        var integrationEvent = new HoleCardsAreMuckedIntegrationEvent()
        {
            HandUid = context.HandUid,
            Nickname = @event.Nickname,
            OccuredAt = @event.OccuredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.HandType.ToRoutingKey()}.hole-cards-are-mucked");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
