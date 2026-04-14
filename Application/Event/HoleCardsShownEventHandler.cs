using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class HoleCardsShownEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HoleCardsShownEvent>
{
    public async Task HandleAsync(HoleCardsShownEvent @event)
    {
        var integrationEvent = new HoleCardsShownIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = @event.HandUid,
            TableUid = @event.TableContext.TableUid,
            TableType = @event.TableContext.TableType.ToString(),
            Nickname = @event.Nickname,
            Cards = @event.Cards,
            Type = @event.Combo.Type.ToString(),
            Weight = @event.Combo.Weight
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{@event.TableContext.TableType.ToRoutingKey()}.hole-cards-shown");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
