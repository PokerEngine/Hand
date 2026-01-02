using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class HoleCardsAreDealtEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HoleCardsAreDealtEvent>
{
    public async Task HandleAsync(HoleCardsAreDealtEvent @event, EventContext context)
    {
        var integrationEvent = new HoleCardsAreDealtIntegrationEvent
        {
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Nickname = @event.Nickname,
            Cards = @event.Cards.ToString(),
            OccurredAt = @event.OccurredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.hole-cards-are-dealt");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
