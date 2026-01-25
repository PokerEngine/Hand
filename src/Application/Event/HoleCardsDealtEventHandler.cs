using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class HoleCardsDealtEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HoleCardsDealtEvent>
{
    public async Task HandleAsync(HoleCardsDealtEvent @event, EventContext context)
    {
        var integrationEvent = new HoleCardsDealtIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Nickname = @event.Nickname,
            Cards = @event.Cards.ToString()
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.hole-cards-dealt");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
