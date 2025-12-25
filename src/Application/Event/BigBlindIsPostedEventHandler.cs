using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class BigBlindIsPostedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<BigBlindIsPostedEvent>
{
    public async Task HandleAsync(BigBlindIsPostedEvent @event, HandUid handUid)
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
