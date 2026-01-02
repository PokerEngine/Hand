using Application.IntegrationEvent;
using Domain.Event;

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
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Nickname = @event.Nickname,
            Amount = @event.Amount,
            OccurredAt = @event.OccurredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.win-without-showdown-is-committed");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
