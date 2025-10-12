using Application.Repository;
using Domain.Entity;
using Domain.Event;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;
using System.Collections.Immutable;

namespace Application.IntegrationEvent;

public class HandCreateIntegrationEventHandler : IIntegrationEventHandler<HandCreateIntegrationEvent>
{
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly IRepository _repository;
    private readonly IRandomizer _randomizer;
    private readonly IEvaluator _evaluator;

    public HandCreateIntegrationEventHandler(
        IIntegrationEventBus integrationEventBus,
        IRepository repository,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        _integrationEventBus = integrationEventBus;
        _repository = repository;
        _randomizer = randomizer;
        _evaluator = evaluator;
    }

    public async Task Handle(HandCreateIntegrationEvent integrationEvent)
    {
        var game = ParseGame(integrationEvent.Game);
        var handUid = new HandUid(integrationEvent.HandUid);
        var smallBlind = new Chips(integrationEvent.SmallBlind);
        var bigBlind = new Chips(integrationEvent.BigBlind);
        var maxSeat = new Seat(integrationEvent.MaxSeat);
        var smallBlindSeat = new Seat(integrationEvent.SmallBlindSeat);
        var bigBlindSeat = new Seat(integrationEvent.BigBlindSeat);
        var buttonSeat = new Seat(integrationEvent.ButtonSeat);
        var participants = integrationEvent.Participants.Select(ParseParticipant).ToImmutableList();

        var eventBus = new EventBus();
        var events = new List<BaseEvent>();
        var listener = (BaseEvent @event) => events.Add(@event);
        eventBus.Subscribe(listener);

        Hand.FromScratch(
            uid: handUid,
            game: game,
            smallBlind: smallBlind,
            bigBlind: bigBlind,
            maxSeat: maxSeat,
            smallBlindSeat: smallBlindSeat,
            bigBlindSeat: bigBlindSeat,
            buttonSeat: buttonSeat,
            participants: participants,
            randomizer: _randomizer,
            evaluator: _evaluator,
            eventBus: eventBus
        );

        eventBus.Unsubscribe(listener);

        await _repository.AddEvents(handUid, events);

        var publisher = new DomainEventPublisher(
            integrationEventBus: _integrationEventBus,
            tableUid: integrationEvent.TableUid,
            handUid: integrationEvent.HandUid
        );
        await publisher.Publish(events);
    }

    private Game ParseGame(string value)
    {
        if (Enum.TryParse(value, out Game game))
        {
            return game;
        }

        throw new ArgumentException("Invalid game", nameof(value));
    }

    private Participant ParseParticipant(IntegrationEventParticipant value)
    {
        return new Participant(
            nickname: new Nickname(value.Nickname),
            seat: new Seat(value.Seat),
            stack: new Chips(value.Stack)
        );
    }
}
