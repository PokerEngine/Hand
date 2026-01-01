using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class HoleCardsAreMuckedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HoleCardsAreMuckedEvent>
{
    public async Task HandleAsync(HoleCardsAreMuckedEvent @event, EventContext context)
    {
        var integrationEvent = new HoleCardsAreMuckedIntegrationEvent()
        {
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Nickname = @event.Nickname,
            OccuredAt = @event.OccuredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.hole-cards-are-mucked");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
