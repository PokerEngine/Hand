using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class BoardCardsDealtEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<BoardCardsDealtEvent>
{
    public async Task HandleAsync(BoardCardsDealtEvent @event, EventContext context)
    {
        var integrationEvent = new BoardCardsDealtIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Cards = @event.Cards.ToString()
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.board-cards-dealt");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
