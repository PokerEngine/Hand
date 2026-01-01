using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class WinWithoutShowdownIsCommittedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<WinWithoutShowdownIsCommittedEvent>
{
    public async Task HandleAsync(WinWithoutShowdownIsCommittedEvent @event, HandUid handUid)
    {
        var integrationEvent = new WinIsCommittedIntegrationEvent
        {
            HandUid = handUid,
            Nickname = @event.Nickname,
            Amount = @event.Amount,
            OccuredAt = @event.OccuredAt
        };

        await integrationEventPublisher.PublishAsync(integrationEvent, "hand.win-without-showdown-is-committed");
    }
}
