using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class HoleCardsDealtEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HoleCardsDealtEvent>
{
    public async Task HandleAsync(HoleCardsDealtEvent @event)
    {
        var integrationEvent = new HoleCardsDealtIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = @event.HandUid,
            TableUid = @event.TableContext.TableUid,
            TableType = @event.TableContext.TableType.ToString(),
            Nickname = @event.Nickname,
            Cards = @event.Cards
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{@event.TableContext.TableType.ToRoutingKey()}.hole-cards-dealt");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
