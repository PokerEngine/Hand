using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class SmallBlindIsPostedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<SmallBlindIsPostedEvent>
{
    public async Task HandleAsync(SmallBlindIsPostedEvent @event, HandUid handUid)
    {
        var integrationEvent = new BlindIsPostedIntegrationEvent
        {
            HandUid = handUid,
            Nickname = @event.Nickname,
            Amount = @event.Amount,
            OccuredAt = @event.OccuredAt
        };

        await integrationEventPublisher.PublishAsync(integrationEvent, IntegrationEventChannel.Hand);
    }
}
