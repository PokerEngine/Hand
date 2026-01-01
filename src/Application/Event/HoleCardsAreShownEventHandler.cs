using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class HoleCardsAreShownEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HoleCardsAreShownEvent>
{
    public async Task HandleAsync(HoleCardsAreShownEvent @event, EventContext context)
    {
        var integrationEvent = new HoleCardsAreShownIntegrationEvent
        {
            HandUid = context.HandUid,
            Nickname = @event.Nickname,
            Cards = @event.Cards.ToString(),
            ComboType = @event.Combo.Type.ToString(),
            ComboWeight = @event.Combo.Weight,
            OccuredAt = @event.OccuredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.HandType.ToRoutingKey()}.hole-cards-are-shown");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
