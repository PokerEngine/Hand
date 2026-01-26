using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class PlayerActionRequestedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<PlayerActionRequestedEvent>
{
    public async Task HandleAsync(PlayerActionRequestedEvent @event, EventContext context)
    {
        var integrationEvent = new PlayerActionRequestedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Nickname = @event.Nickname,
            FoldIsAvailable = @event.FoldIsAvailable,
            CheckIsAvailable = @event.CheckIsAvailable,
            CallIsAvailable = @event.CallIsAvailable,
            CallByAmount = @event.CallByAmount,
            RaiseIsAvailable = @event.RaiseIsAvailable,
            MinRaiseByAmount = @event.MinRaiseByAmount,
            MaxRaiseByAmount = @event.MaxRaiseByAmount
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.player-action-requested");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
