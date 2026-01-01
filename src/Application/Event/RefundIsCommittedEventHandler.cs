using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class RefundIsCommittedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<RefundIsCommittedEvent>
{
    public async Task HandleAsync(RefundIsCommittedEvent @event, HandUid handUid)
    {
        var integrationEvent = new RefundIsCommittedIntegrationEvent
        {
            HandUid = handUid,
            Nickname = @event.Nickname,
            Amount = @event.Amount,
            OccuredAt = @event.OccuredAt
        };

        await integrationEventPublisher.PublishAsync(integrationEvent, "hand.refund-is-committed");
    }
}
