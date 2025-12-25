using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class DecisionIsRequestedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<DecisionIsRequestedEvent>
{
    public async Task HandleAsync(DecisionIsRequestedEvent @event, HandUid handUid)
    {
        var integrationEvent = new DecisionIsRequestedIntegrationEvent
        {
            HandUid = handUid,
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

        await integrationEventPublisher.PublishAsync(integrationEvent, IntegrationEventChannel.Hand);
    }
}
