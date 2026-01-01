using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class BoardCardsAreDealtEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<BoardCardsAreDealtEvent>
{
    public async Task HandleAsync(BoardCardsAreDealtEvent @event, EventContext context)
    {
        var integrationEvent = new BoardCardsAreDealtIntegrationEvent
        {
            HandUid = context.HandUid,
            Cards = @event.Cards.ToString(),
            OccuredAt = @event.OccuredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.HandType.ToRoutingKey()}.board-cards-are-dealt");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
