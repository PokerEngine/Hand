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

    public Hand(
        HandUid uid,
        Game game,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        IList<IDealer> dealers,
        EventBus eventBus
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

        var participants = table.Select(x => new Participant(x.Nickname, x.Position, x.Stake)).ToList();
        var @event = new HandIsCreatedEvent(
            Game: game,
            SmallBlind: pot.SmallBlind,
            BigBlind: pot.BigBlind,
            Participants: participants,
            HandUid: uid,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(@event);
    }

    public void Connect(Nickname nickname, EventBus eventBus)
    {
        var player = _table.GetPlayerByNickname(nickname);
        player.Connect();

        var @event = new PlayerConnectedEvent(Nickname: nickname, HandUid: Uid, OccuredAt: DateTime.Now);
        eventBus.Publish(@event);
    }

    public void Disconnect(Nickname nickname, EventBus eventBus)
    {
        var player = _table.GetPlayerByNickname(nickname);
        player.Disconnect();

        var @event = new PlayerDisconnectedEvent(Nickname: nickname, HandUid: Uid, OccuredAt: DateTime.Now);
        eventBus.Publish(@event);
    }

    public void Start(EventBus eventBus)
    {
        var @event = new HandIsStartedEvent(HandUid: Uid, OccuredAt: DateTime.Now);
        eventBus.Publish(@event);

        var listener = (StageIsFinishedEvent e) => StartNextDealerOrFinish(eventBus);
        eventBus.Subscribe(listener);

        _dealer.Start(
            handUid: Uid,
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
            handUid: Uid,
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
            var @event = new HandIsFinishedEvent(HandUid: Uid, OccuredAt: DateTime.Now);
            eventBus.Publish(@event);
        }
        else
        {
            _dealerIdx++;
            _dealer.Start(
                handUid: Uid,
                table: _table,
                pot: _pot,
                deck: _deck,
                evaluator: _evaluator,
                eventBus: eventBus
            );
        }
    }
}
