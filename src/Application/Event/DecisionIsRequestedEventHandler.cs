using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class DecisionIsRequestedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<DecisionIsRequestedEvent>
{
    public async Task HandleAsync(DecisionIsRequestedEvent @event, EventContext context)
    {
        var integrationEvent = new DecisionIsRequestedIntegrationEvent
        {
            HandUid = context.HandUid,
            Nickname = @event.Nickname,
            FoldIsAvailable = @event.FoldIsAvailable,
            CheckIsAvailable = @event.CheckIsAvailable,
            CallIsAvailable = @event.CallIsAvailable,
            CallToAmount = @event.CallToAmount,
            RaiseIsAvailable = @event.RaiseIsAvailable,
            MinRaiseToAmount = @event.MinRaiseToAmount,
            MaxRaiseToAmount = @event.MaxRaiseToAmount,
            OccuredAt = @event.OccuredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.HandType.ToRoutingKey()}.decision-is-requested");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
