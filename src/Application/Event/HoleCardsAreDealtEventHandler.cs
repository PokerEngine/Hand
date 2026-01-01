using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

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
            Nickname = @event.Nickname,
            Cards = @event.Cards.ToString(),
            OccuredAt = @event.OccuredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.HandType.ToRoutingKey()}.hole-cards-are-dealt");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
