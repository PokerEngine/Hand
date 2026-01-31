using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class PlayerActedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<PlayerActedEvent>
{
    public async Task HandleAsync(PlayerActedEvent @event, EventContext context)
    {
        var integrationEvent = new PlayerActedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Nickname = @event.Nickname,
            Type = @event.Action.Type.ToString(),
            Amount = @event.Action.Amount
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.player-acted");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
