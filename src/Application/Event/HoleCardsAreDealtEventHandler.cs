using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class HoleCardsAreDealtEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HoleCardsAreDealtEvent>
{
    public async Task HandleAsync(HoleCardsAreDealtEvent @event, HandUid handUid)
    {
        var integrationEvent = new HoleCardsAreDealtIntegrationEvent
        {
            HandUid = handUid,
            Nickname = @event.Nickname,
            Cards = @event.Cards.ToString(),
            OccuredAt = @event.OccuredAt
        };

        await integrationEventPublisher.PublishAsync(integrationEvent, IntegrationEventChannel.Hand);
    }
}
