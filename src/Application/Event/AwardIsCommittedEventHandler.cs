using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class AwardIsCommittedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<AwardIsCommittedEvent>
{
    public async Task HandleAsync(AwardIsCommittedEvent @event, EventContext context)
    {
        var integrationEvent = new AwardIsCommittedIntegrationEvent
        {
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Nicknames = @event.Nicknames.Select(n => n.ToString()).ToList(),
            Amount = @event.Amount,
            OccurredAt = @event.OccurredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.award-is-committed");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
