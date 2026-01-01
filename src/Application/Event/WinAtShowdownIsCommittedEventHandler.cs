using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class WinAtShowdownIsCommittedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<WinAtShowdownIsCommittedEvent>
{
    public async Task HandleAsync(WinAtShowdownIsCommittedEvent @event, HandUid handUid)
    {
        foreach (var (nickname, amount) in @event.WinPot)
        {
            var integrationEvent = new WinIsCommittedIntegrationEvent
            {
                HandUid = handUid,
                Nickname = nickname,
                Amount = amount,
                OccuredAt = @event.OccuredAt
            };
            await integrationEventPublisher.PublishAsync(integrationEvent, "hand.win-at-showdown-is-committed");
        }
    }
}
