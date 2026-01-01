using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class DecisionIsCommittedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<DecisionIsCommittedEvent>
{
    public async Task HandleAsync(DecisionIsCommittedEvent @event, EventContext context)
    {
        var integrationEvent = new DecisionIsCommittedIntegrationEvent()
        {
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Nickname = @event.Nickname,
            DecisionType = @event.Decision.Type.ToString(),
            DecisionAmount = @event.Decision.Amount,
            OccuredAt = @event.OccuredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.decision-is-committed");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
