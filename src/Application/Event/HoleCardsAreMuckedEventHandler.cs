using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class HoleCardsAreMuckedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HoleCardsAreMuckedEvent>
{
    public async Task HandleAsync(HoleCardsAreMuckedEvent @event, HandUid handUid)
    {
        var integrationEvent = new HoleCardsAreMuckedIntegrationEvent()
        {
            HandUid = handUid,
            Nickname = @event.Nickname,
            OccuredAt = @event.OccuredAt
        };

        await integrationEventPublisher.PublishAsync(integrationEvent, "hand.hole-cards-are-mucked");
    }
}
