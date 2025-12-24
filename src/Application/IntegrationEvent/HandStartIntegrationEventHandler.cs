using Application.Repository;
using Domain.Entity;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Application.IntegrationEvent;

public class HandStartIntegrationEventHandler(
    IIntegrationEventBus integrationEventBus,
    IRepository repository,
    IRandomizer randomizer,
    IEvaluator evaluator
) : IIntegrationEventHandler<HandStartIntegrationEvent>
{
    public async Task Handle(HandStartIntegrationEvent integrationEvent)
    {
        var handUid = new HandUid(integrationEvent.HandUid);

        var hand = Hand.FromEvents(
            uid: handUid,
            randomizer: randomizer,
            evaluator: evaluator,
            events: await repository.GetEvents(handUid)
        );

        hand.Start();

        var events = hand.PullEvents();
        await repository.AddEvents(handUid, events);

        var publisher = new DomainEventPublisher(
            integrationEventBus: integrationEventBus,
            tableUid: integrationEvent.TableUid,
            handUid: integrationEvent.HandUid
        );
        await publisher.Publish(events);
    }
}
