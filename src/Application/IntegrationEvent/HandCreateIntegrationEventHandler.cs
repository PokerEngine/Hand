using Application.Repository;
using Domain.Entity;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Application.IntegrationEvent;

public class HandCreateIntegrationEventHandler(
    IIntegrationEventBus integrationEventBus,
    IRepository repository,
    IRandomizer randomizer,
    IEvaluator evaluator
) : IIntegrationEventHandler<HandCreateIntegrationEvent>
{
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
        var participants = integrationEvent.Participants.Select(ParseParticipant).ToList();

        var hand = Hand.FromScratch(
            uid: handUid,
            game: game,
            smallBlind: smallBlind,
            bigBlind: bigBlind,
            maxSeat: maxSeat,
            smallBlindSeat: smallBlindSeat,
            bigBlindSeat: bigBlindSeat,
            buttonSeat: buttonSeat,
            participants: participants,
            randomizer: randomizer,
            evaluator: evaluator
        );

        var events = hand.PullEvents();
        await repository.AddEvents(handUid, events);

        var publisher = new DomainEventPublisher(
            integrationEventBus: integrationEventBus,
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
