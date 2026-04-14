using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class PlayerActedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<PlayerActedEvent>
{
    public async Task HandleAsync(PlayerActedEvent @event)
    {
        var integrationEvent = new PlayerActedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = @event.HandUid,
            TableUid = @event.TableContext.TableUid,
            TableType = @event.TableContext.TableType.ToString(),
            Nickname = @event.Nickname,
            Type = @event.Action.Type.ToString(),
            Amount = @event.Action.Amount
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{@event.TableContext.TableType.ToRoutingKey()}.player-acted");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
