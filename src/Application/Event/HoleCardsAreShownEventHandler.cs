using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class HoleCardsAreShownEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HoleCardsAreShownEvent>
{
    public async Task HandleAsync(HoleCardsAreShownEvent @event, HandUid handUid)
    {
        var integrationEvent = new HoleCardsAreShownIntegrationEvent
        {
            HandUid = handUid,
            Nickname = @event.Nickname,
            Cards = @event.Cards.ToString(),
            ComboType = @event.Combo.Type.ToString(),
            ComboWeight = @event.Combo.Weight,
            OccuredAt = @event.OccuredAt
        };

        await integrationEventPublisher.PublishAsync(integrationEvent, "hand.hole-cards-are-shown");
    }
}
