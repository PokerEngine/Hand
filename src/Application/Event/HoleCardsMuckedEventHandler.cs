using Application.IntegrationEvent;
using Domain.Event;

namespace Application.Event;

public class HoleCardsMuckedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HoleCardsMuckedEvent>
{
    public async Task HandleAsync(HoleCardsMuckedEvent @event, EventContext context)
    {
        var integrationEvent = new HoleCardsMuckedIntegrationEvent()
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Nickname = @event.Nickname
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.hole-cards-mucked");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }
}
