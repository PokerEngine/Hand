using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class WinIsCommittedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<WinIsCommittedEvent>
{
    public async Task HandleAsync(WinIsCommittedEvent @event, EventContext context)
    {
        var integrationEvent = new WinIsCommittedIntegrationEvent
        {
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Nicknames = @event.Nicknames.Select(n => n.ToString()).ToList(),
            Amount = @event.Amount,
            OccurredAt = @event.OccurredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.win-is-committed");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
