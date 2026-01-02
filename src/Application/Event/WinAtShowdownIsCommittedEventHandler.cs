using Application.IntegrationEvent;
using Domain.Event;

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
                TableUid = context.TableUid,
                TableType = context.TableType.ToString(),
                Nickname = nickname,
                Amount = amount,
                OccurredAt = @event.OccurredAt
            };

            var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.win-at-showdown-is-committed");
            await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
        }
    }
}
