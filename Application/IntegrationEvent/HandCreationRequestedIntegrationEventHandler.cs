using Domain.Entity;
using Domain.Error;
using Domain.Event;
using Domain.ValueObject;

namespace Application.IntegrationEvent;

public class HandCreationRequestedIntegrationEventHandler : IIntegrationEventHandler<HandCreationRequestedIntegrationEvent>
{
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly IRepository _repository;

    public HandCreationRequestedIntegrationEventHandler(
        IIntegrationEventBus integrationEventBus,
        IRepository repository
    )
    {
        _integrationEventBus = integrationEventBus;
        _repository = repository;
    }

    public void Handle(HandCreationRequestedIntegrationEvent integrationEvent)
    {
        var game = ParseGame(integrationEvent.Game);
        var handUid = new HandUid(integrationEvent.HandUid);
        var smallBlind = new Chips(integrationEvent.SmallBlind);
        var bigBlind = new Chips(integrationEvent.BigBlind);
        var participants = integrationEvent.Participants.Select(ParseParticipant).ToList();

        var eventBus = new EventBus();
        var events = new List<IEvent>();
        var listener = (IEvent @event) => events.Add(@event);
        eventBus.Subscribe(listener);

        Hand.FromScratch(
            uid: handUid,
            game: game,
            smallBlind: smallBlind,
            bigBlind: bigBlind,
            participants: participants,
            eventBus: eventBus
        );

        eventBus.Unsubscribe(listener);

        _repository.AddEvents(handUid, events);

        var publisher = new DomainEventPublisher(
            integrationEventBus: _integrationEventBus,
            tableUid: integrationEvent.TableUid,
            handUid: integrationEvent.HandUid
        );
        publisher.Publish(events);
    }

    private Game ParseGame(string value)
    {
        if (Enum.TryParse(value, out Game game))
        {
            return game;
        }

        throw new NotValidError($"Invalid game: {value}");
    }

    private Participant ParseParticipant(IntegrationEventParticipant value)
    {
        return new Participant(
            nickname: new Nickname(value.Nickname),
            position: ParsePosition(value.Position),
            stake: new Chips(value.Stake)
        );
    }

    private Position ParsePosition(string value)
    {
        if (Enum.TryParse(value, out Position position))
        {
            return position;
        }

        throw new NotValidError($"Invalid position: {value}");
    }
}