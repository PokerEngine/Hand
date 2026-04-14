using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class BoardCardsDealtEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<BoardCardsDealtEvent>
{
    public async Task HandleAsync(BoardCardsDealtEvent @event)
    {
        var integrationEvent = new BoardCardsDealtIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = @event.HandUid,
            TableUid = @event.TableContext.TableUid,
            TableType = @event.TableContext.TableType.ToString(),
            Cards = @event.Cards
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{@event.TableContext.TableType.ToRoutingKey()}.board-cards-dealt");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
