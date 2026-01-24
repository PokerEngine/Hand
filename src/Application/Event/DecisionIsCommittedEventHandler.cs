using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class DecisionIsCommittedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<DecisionIsCommittedEvent>
{
    public async Task HandleAsync(DecisionIsCommittedEvent @event, EventContext context)
    {
        var integrationEvent = new DecisionIsCommittedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Nickname = @event.Nickname,
            Type = @event.Decision.Type.ToString(),
            Amount = @event.Decision.Amount
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.decision-is-committed");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
