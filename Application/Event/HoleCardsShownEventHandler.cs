using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class HoleCardsShownEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HoleCardsShownEvent>
{
    public async Task HandleAsync(HoleCardsShownEvent @event, EventContext context)
    {
        var integrationEvent = new HoleCardsShownIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Nickname = @event.Nickname,
            Cards = @event.Cards,
            Type = @event.Combo.Type.ToString(),
            Weight = @event.Combo.Weight
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.hole-cards-shown");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
