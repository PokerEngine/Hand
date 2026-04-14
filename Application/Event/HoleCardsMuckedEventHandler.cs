using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class HoleCardsMuckedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HoleCardsMuckedEvent>
{
    public async Task HandleAsync(HoleCardsMuckedEvent @event)
    {
        var integrationEvent = new HoleCardsMuckedIntegrationEvent()
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = @event.HandUid,
            TableUid = @event.TableContext.TableUid,
            TableType = @event.TableContext.TableType.ToString(),
            Nickname = @event.Nickname
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{@event.TableContext.TableType.ToRoutingKey()}.hole-cards-mucked");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
