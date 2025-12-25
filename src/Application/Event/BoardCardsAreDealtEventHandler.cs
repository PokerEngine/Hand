using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class BoardCardsAreDealtEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<BoardCardsAreDealtEvent>
{
    public async Task HandleAsync(BoardCardsAreDealtEvent @event, HandUid handUid)
    {
        var integrationEvent = new BoardCardsAreDealtIntegrationEvent
        {
            HandUid = handUid,
            Cards = @event.Cards.ToString(),
            OccuredAt = @event.OccuredAt
        };

        await integrationEventPublisher.PublishAsync(integrationEvent, IntegrationEventChannel.Hand);
    }
}
