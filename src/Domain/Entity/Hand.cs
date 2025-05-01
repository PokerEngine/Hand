using Domain.Entity.Factory;
using Domain.Error;
using Domain.Event;
using Domain.Service.Dealer;
using Domain.Service.Evaluator;
using Domain.ValueObject;
using System.Collections.Immutable;

namespace Domain.Entity;

public class Hand
{
    public readonly HandUid Uid;
    public readonly Game Game;
    public readonly BaseTable Table;
    public readonly BasePot Pot;
    public readonly BaseDeck Deck;
    private readonly IEvaluator _evaluator;
    private readonly ImmutableList<IDealer> _dealers;

    private int _dealerIdx;
    private IDealer Dealer => _dealers[_dealerIdx];

    private Hand(
        HandUid uid,
        Game game,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        IEnumerable<IDealer> dealers
    )
    {
        Uid = uid;
        Game = game;
        Table = table;
        Pot = pot;
        Deck = deck;
        _evaluator = evaluator;
        _dealers = dealers.ToImmutableList();
        _dealerIdx = 0;
    }

    public static Hand FromScratch(
        HandUid uid,
        Game game,
        Chips smallBlind,
        Chips bigBlind,
        List<Participant> participants,
        IEventBus eventBus
    )
    {
        var factory = FactoryRegistry.GetFactory(game);
        var hand = new Hand(
            uid: uid,
            game: game,
            table: factory.GetTable(participants),
            pot: factory.GetPot(smallBlind, bigBlind),
            deck: factory.GetDeck(),
            evaluator: factory.GetEvaluator(),
            dealers: factory.GetDealers()
        );

        var @event = new HandIsCreatedEvent(
            Game: game,
            SmallBlind: smallBlind,
            BigBlind: bigBlind,
            Participants: participants,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(@event);

        return hand;
    }

    public static Hand FromEvents(HandUid uid, IList<IEvent> events)
    {
        if (events.Count == 0 || events[0] is not HandIsCreatedEvent)
        {
            throw new NotAvailableError("The first event must be a HandIsCreatedEvent");
        }

        var eventBus = new EventBus();

        var createdEvent = (HandIsCreatedEvent)events[0];
        var hand = FromScratch(
            uid: uid,
            game: createdEvent.Game,
            smallBlind: createdEvent.SmallBlind,
            bigBlind: createdEvent.BigBlind,
            participants: createdEvent.Participants,
            eventBus: eventBus
        );

        foreach (var @event in events)
        {
            switch (@event)
            {
                case HandIsCreatedEvent:
                    break;
                case HandIsStartedEvent:
                    break;
                case HandIsFinishedEvent:
                    break;
                case PlayerConnectedEvent e:
                    hand.ConnectPlayer(e.Nickname, eventBus);
                    break;
                case PlayerDisconnectedEvent e:
                    hand.DisconnectPlayer(e.Nickname, eventBus);
                    break;
                default:
                    hand.Dealer.Handle(
                        @event: @event,
                        table: hand.Table,
                        pot: hand.Pot,
                        deck: hand.Deck,
                        evaluator: hand._evaluator
                    );
                    if (@event is StageIsFinishedEvent && hand._dealerIdx < hand._dealers.Count - 1) hand._dealerIdx++;
                    break;
            }
        }

        return hand;
    }

    public void Start(IEventBus eventBus)
    {
        var @event = new HandIsStartedEvent(OccuredAt: DateTime.Now);
        eventBus.Publish(@event);

        var listener = (StageIsFinishedEvent e) => StartNextDealerOrFinish(eventBus);
        eventBus.Subscribe(listener);

        Dealer.Start(
            table: Table,
            pot: Pot,
            deck: Deck,
            evaluator: _evaluator,
            eventBus: eventBus
        );

        eventBus.Unsubscribe(listener);
    }

    public void ConnectPlayer(Nickname nickname, IEventBus eventBus)
    {
        var player = Table.GetPlayerByNickname(nickname);
        player.Connect();

        var @event = new PlayerConnectedEvent(Nickname: nickname, OccuredAt: DateTime.Now);
        eventBus.Publish(@event);
    }

    public void DisconnectPlayer(Nickname nickname, IEventBus eventBus)
    {
        var player = Table.GetPlayerByNickname(nickname);
        player.Disconnect();

        var @event = new PlayerDisconnectedEvent(Nickname: nickname, OccuredAt: DateTime.Now);
        eventBus.Publish(@event);
    }

    public void CommitDecision(Nickname nickname, Decision decision, IEventBus eventBus)
    {
        var listener = (StageIsFinishedEvent e) => StartNextDealerOrFinish(eventBus);
        eventBus.Subscribe(listener);

        Dealer.CommitDecision(
            nickname: nickname,
            decision: decision,
            table: Table,
            pot: Pot,
            deck: Deck,
            evaluator: _evaluator,
            eventBus: eventBus
        );

        eventBus.Unsubscribe(listener);
    }

    private void StartNextDealerOrFinish(IEventBus eventBus)
    {
        if (_dealerIdx == _dealers.Count - 1)
        {
            var @event = new HandIsFinishedEvent(OccuredAt: DateTime.Now);
            eventBus.Publish(@event);
        }
        else
        {
            _dealerIdx++;
            Dealer.Start(
                table: Table,
                pot: Pot,
                deck: Deck,
                evaluator: _evaluator,
                eventBus: eventBus
            );
        }
    }
}
