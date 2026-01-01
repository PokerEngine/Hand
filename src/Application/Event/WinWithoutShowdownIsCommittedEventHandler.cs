using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class WinWithoutShowdownIsCommittedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<WinWithoutShowdownIsCommittedEvent>
{
    public async Task HandleAsync(WinWithoutShowdownIsCommittedEvent @event, EventContext context)
    {
        var integrationEvent = new WinIsCommittedIntegrationEvent
        {
            HandUid = context.HandUid,
            Nickname = @event.Nickname,
            Amount = @event.Amount,
            OccuredAt = @event.OccuredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.HandType.ToRoutingKey()}.win-without-showdown-is-committed");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
