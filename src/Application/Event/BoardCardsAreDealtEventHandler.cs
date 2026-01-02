using Application.IntegrationEvent;
using Domain.Event;

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
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Cards = @event.Cards.ToString(),
            OccurredAt = @event.OccurredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.board-cards-are-dealt");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
