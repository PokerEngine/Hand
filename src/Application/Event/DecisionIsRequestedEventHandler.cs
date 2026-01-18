using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class DecisionIsRequestedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<DecisionIsRequestedEvent>
{
    public async Task HandleAsync(DecisionIsRequestedEvent @event, EventContext context)
    {
        var integrationEvent = new DecisionIsRequestedIntegrationEvent
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
            CallToAmount = @event.CallToAmount,
            RaiseIsAvailable = @event.RaiseIsAvailable,
            MinRaiseToAmount = @event.MinRaiseToAmount,
            MaxRaiseToAmount = @event.MaxRaiseToAmount
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.decision-is-requested");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
