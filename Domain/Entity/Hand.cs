using Domain.Entity.Factory;
using Domain.Event;
using Domain.Service.Dealer;
using Domain.Service.Evaluator;
using Domain.ValueObject;

namespace Domain.Entity;

public class Hand
{
    public readonly HandUid Uid;
    public readonly Game Game;
    private BaseTable _table;
    private BasePot _pot;
    private BaseDeck _deck;
    private IEvaluator _evaluator;
    private IList<IDealer> _dealers;
    private int _dealerIdx;
    private IDealer _dealer => _dealers[_dealerIdx];

    private Hand(
        HandUid uid,
        Game game,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        IList<IDealer> dealers
    )
    {
        Uid = uid;
        Game = game;
        _table = table;
        _pot = pot;
        _deck = deck;
        _evaluator = evaluator;
        _dealers = dealers;
        _dealerIdx = 0;
    }

    public static Hand FromScratch(
        HandUid uid,
        Game game,
        Chips smallBlind,
        Chips bigBlind,
        List<Participant> participants,
        EventBus eventBus
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

    public void Connect(Nickname nickname, EventBus eventBus)
    {
        var player = _table.GetPlayerByNickname(nickname);
        player.Connect();

        var @event = new PlayerConnectedEvent(Nickname: nickname, OccuredAt: DateTime.Now);
        eventBus.Publish(@event);
    }

    public void Disconnect(Nickname nickname, EventBus eventBus)
    {
        var player = _table.GetPlayerByNickname(nickname);
        player.Disconnect();

        var @event = new PlayerDisconnectedEvent(Nickname: nickname, OccuredAt: DateTime.Now);
        eventBus.Publish(@event);
    }

    public void Start(EventBus eventBus)
    {
        var @event = new HandIsStartedEvent(OccuredAt: DateTime.Now);
        eventBus.Publish(@event);

        var listener = (StageIsFinishedEvent e) => StartNextDealerOrFinish(eventBus);
        eventBus.Subscribe(listener);

        _dealer.Start(
            table: _table,
            pot: _pot,
            deck: _deck,
            evaluator: _evaluator,
            eventBus: eventBus
        );

        eventBus.Unsubscribe(listener);
    }

    public void CommitDecision(Nickname nickname, Decision decision, EventBus eventBus)
    {
        var listener = (StageIsFinishedEvent e) => StartNextDealerOrFinish(eventBus);
        eventBus.Subscribe(listener);

        _dealer.CommitDecision(
            nickname: nickname,
            decision: decision,
            table: _table,
            pot: _pot,
            deck: _deck,
            evaluator: _evaluator,
            eventBus: eventBus
        );

        eventBus.Unsubscribe(listener);
    }

    private void StartNextDealerOrFinish(EventBus eventBus)
    {
        if (_dealerIdx == _dealers.Count - 1)
        {
            var @event = new HandIsFinishedEvent(OccuredAt: DateTime.Now);
            eventBus.Publish(@event);
        }
        else
        {
            _dealerIdx++;
            _dealer.Start(
                table: _table,
                pot: _pot,
                deck: _deck,
                evaluator: _evaluator,
                eventBus: eventBus
            );
        }
    }
}
