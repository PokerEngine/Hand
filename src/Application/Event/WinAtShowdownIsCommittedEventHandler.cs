using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class WinAtShowdownIsCommittedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<WinAtShowdownIsCommittedEvent>
{
    public async Task HandleAsync(WinAtShowdownIsCommittedEvent @event, EventContext context)
    {
        foreach (var (nickname, amount) in @event.WinPot)
        {
            var integrationEvent = new WinIsCommittedIntegrationEvent
            {
                HandUid = context.HandUid,
                Nickname = nickname,
                Amount = amount,
                OccuredAt = @event.OccuredAt
            };

            var routingKey = new IntegrationEventRoutingKey($"hand.{context.HandType.ToRoutingKey()}.win-at-showdown-is-committed");
            await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
        }
    }
}
