using Domain.Entity.Factory;
using Domain.Event;
using Domain.Service.Dealer;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;
using System.Collections.Immutable;

namespace Domain.Entity;

public class Hand
{
    public readonly HandUid Uid;
    public readonly TableUid TableUid;
    public readonly TableType TableType;
    public readonly Game Game;
    public readonly Table Table;
    public readonly BasePot Pot;
    public readonly BaseDeck Deck;
    private readonly IRandomizer _randomizer;
    private readonly IEvaluator _evaluator;
    private readonly ImmutableList<IDealer> _dealers;

    private List<IEvent> _events;

    private int _dealerIdx;
    private IDealer Dealer => _dealers[_dealerIdx];

    private Hand(
        HandUid uid,
        TableUid tableUid,
        TableType tableType,
        Game game,
        Table table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator,
        IEnumerable<IDealer> dealers
    )
    {
        Uid = uid;
        TableUid = tableUid;
        TableType = tableType;
        Game = game;
        Table = table;
        Pot = pot;
        Deck = deck;
        _randomizer = randomizer;
        _evaluator = evaluator;
        _dealers = dealers.ToImmutableList();
        _dealerIdx = 0;
        _events = [];
    }

    public static Hand FromScratch(
        HandUid uid,
        TableUid tableUid,
        TableType tableType,
        Game game,
        Chips smallBlind,
        Chips bigBlind,
        Seat maxSeat,
        Seat smallBlindSeat,
        Seat bigBlindSeat,
        Seat buttonSeat,
        List<Participant> participants,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        var factory = FactoryRegistry.GetFactory(game);
        var hand = new Hand(
            uid: uid,
            tableUid: tableUid,
            tableType: tableType,
            game: game,
            table: factory.GetTable(
                participants: participants,
                maxSeat: maxSeat,
                smallBlindSeat: smallBlindSeat,
                bigBlindSeat: bigBlindSeat,
                buttonSeat: buttonSeat
            ),
            pot: factory.GetPot(
                smallBlind: smallBlind,
                bigBlind: bigBlind
            ),
            deck: factory.GetDeck(),
            randomizer: randomizer,
            evaluator: evaluator,
            dealers: factory.GetDealers()
        );

        var @event = new HandIsCreatedEvent
        {
            TableUid = tableUid,
            Game = game,
            TableType = tableType,
            SmallBlind = smallBlind,
            BigBlind = bigBlind,
            MaxSeat = maxSeat,
            SmallBlindSeat = smallBlindSeat,
            BigBlindSeat = bigBlindSeat,
            ButtonSeat = buttonSeat,
            Participants = participants,
            OccurredAt = DateTime.Now
        };
        hand.AddEvent(@event);

        return hand;
    }

    public static Hand FromEvents(
        HandUid uid,
        IRandomizer randomizer,
        IEvaluator evaluator,
        List<IEvent> events
    )
    {
        if (events.Count == 0 || events[0] is not HandIsCreatedEvent)
        {
            throw new InvalidOperationException("The first event must be a HandIsCreatedEvent");
        }

        var createdEvent = (HandIsCreatedEvent)events[0];
        var hand = FromScratch(
            uid: uid,
            tableUid: createdEvent.TableUid,
            tableType: createdEvent.TableType,
            game: createdEvent.Game,
            smallBlind: createdEvent.SmallBlind,
            bigBlind: createdEvent.BigBlind,
            maxSeat: createdEvent.MaxSeat,
            smallBlindSeat: createdEvent.SmallBlindSeat,
            bigBlindSeat: createdEvent.BigBlindSeat,
            buttonSeat: createdEvent.ButtonSeat,
            participants: createdEvent.Participants,
            randomizer: randomizer,
            evaluator: evaluator
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
                default:
                    hand.Dealer.Handle(
                        @event: @event,
                        game: hand.Game,
                        table: hand.Table,
                        pot: hand.Pot,
                        deck: hand.Deck,
                        randomizer: hand._randomizer,
                        evaluator: hand._evaluator
                    );
                    if (@event is StageIsFinishedEvent && hand._dealerIdx < hand._dealers.Count - 1) hand._dealerIdx++;
                    break;
            }
        }

        return hand;
    }

    public State GetState()
    {
        var playerStates = Table.Players.Select((p) => new StatePlayer
        {
            Nickname = p.Nickname,
            Seat = p.Seat,
            Stack = p.Stack,
            HoleCards = p.HoleCards,
            IsFolded = p.IsFolded
        }).ToList();

        return new State
        {
            Players = playerStates,
            BoardCards = Table.BoardCards,
            CurrentSidePot = Pot.CurrentSidePot,
            PreviousSidePot = Pot.PreviousSidePot
        };
    }

    public void Start()
    {
        var @event = new HandIsStartedEvent
        {
            OccurredAt = DateTime.Now
        };
        AddEvent(@event);

        StartDealer();
    }

    public void CommitDecision(Nickname nickname, Decision decision)
    {
        var events = Dealer.CommitDecision(
            nickname: nickname,
            decision: decision,
            game: Game,
            table: Table,
            pot: Pot,
            deck: Deck,
            randomizer: _randomizer,
            evaluator: _evaluator
        );
        foreach (var e in events)
        {
            AddEvent(e);

            if (e is StageIsFinishedEvent)
            {
                StartNextDealerOrFinish();
                break;
            }
        }
    }

    private void StartNextDealerOrFinish()
    {
        if (_dealerIdx == _dealers.Count - 1)
        {
            var @event = new HandIsFinishedEvent
            {
                OccurredAt = DateTime.Now
            };
            AddEvent(@event);
        }
        else
        {
            _dealerIdx++;
            StartDealer();
        }
    }

    private void StartDealer()
    {
        var events = Dealer.Start(
            game: Game,
            table: Table,
            pot: Pot,
            deck: Deck,
            randomizer: _randomizer,
            evaluator: _evaluator
        );
        foreach (var e in events)
        {
            AddEvent(e);

            if (e is StageIsFinishedEvent)
            {
                StartNextDealerOrFinish();
                break;
            }
        }
    }

    #region Events

    public List<IEvent> PullEvents()
    {
        var events = _events.ToList();
        _events.Clear();

        return events;
    }

    private void AddEvent(IEvent @event)
    {
        _events.Add(@event);
    }

    #endregion
}
