using Domain.Entity.Factory;
using Domain.Event;
using Domain.Exception;
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
    public readonly Rules Rules;
    public readonly Table Table;
    public readonly Pot Pot;
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
        Rules rules,
        Table table,
        Pot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator,
        IEnumerable<IDealer> dealers
    )
    {
        Uid = uid;
        TableUid = tableUid;
        TableType = tableType;
        Rules = rules;
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
        Rules rules,
        Positions positions,
        List<Participant> participants,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        var factory = FactoryRegistry.GetFactory(rules.Game);
        var hand = new Hand(
            uid: uid,
            tableUid: tableUid,
            tableType: tableType,
            rules: rules,
            table: factory.GetTable(
                participants: participants,
                rules: rules,
                positions: positions
            ),
            pot: factory.GetPot(rules),
            deck: factory.GetDeck(rules),
            randomizer: randomizer,
            evaluator: evaluator,
            dealers: factory.GetDealers(rules)
        );

        var @event = new HandStartedEvent
        {
            TableUid = tableUid,
            TableType = tableType,
            Rules = rules,
            Positions = positions,
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
        if (events.Count == 0 || events[0] is not HandStartedEvent)
        {
            throw new InvalidHandStateException("The first event must be a HandStartedEvent");
        }

        var createdEvent = (HandStartedEvent)events[0];
        var factory = FactoryRegistry.GetFactory(createdEvent.Rules.Game);
        var hand = new Hand(
            uid: uid,
            tableUid: createdEvent.TableUid,
            tableType: createdEvent.TableType,
            rules: createdEvent.Rules,
            table: factory.GetTable(
                participants: createdEvent.Participants,
                rules: createdEvent.Rules,
                positions: createdEvent.Positions
            ),
            pot: factory.GetPot(createdEvent.Rules),
            deck: factory.GetDeck(createdEvent.Rules),
            randomizer: randomizer,
            evaluator: evaluator,
            dealers: factory.GetDealers(createdEvent.Rules)
        );

        foreach (var @event in events)
        {
            switch (@event)
            {
                case HandStartedEvent:
                    break;
                case HandFinishedEvent:
                    break;
                default:
                    hand.Dealer.Handle(
                        @event: @event,
                        rules: hand.Rules,
                        table: hand.Table,
                        pot: hand.Pot,
                        deck: hand.Deck,
                        randomizer: hand._randomizer,
                        evaluator: hand._evaluator
                    );
                    if (@event is StageFinishedEvent && hand._dealerIdx < hand._dealers.Count - 1) hand._dealerIdx++;
                    break;
            }
        }

        return hand;
    }

    public State GetState()
    {
        return new State
        {
            Rules = Rules,
            Table = Table.GetState(),
            Pot = Pot.GetState()
        };
    }

    public void Start()
    {
        StartDealer();
    }

    public void SubmitPlayerAction(Nickname nickname, PlayerAction action)
    {
        var events = Dealer.SubmitPlayerAction(
            nickname: nickname,
            action: action,
            rules: Rules,
            table: Table,
            pot: Pot,
            deck: Deck,
            randomizer: _randomizer,
            evaluator: _evaluator
        );
        foreach (var e in events)
        {
            AddEvent(e);

            if (e is StageFinishedEvent)
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
            var @event = new HandFinishedEvent
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
            rules: Rules,
            table: Table,
            pot: Pot,
            deck: Deck,
            randomizer: _randomizer,
            evaluator: _evaluator
        );
        foreach (var e in events)
        {
            AddEvent(e);

            if (e is StageFinishedEvent)
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
