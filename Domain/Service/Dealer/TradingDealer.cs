using Domain.Entity;
using Domain.Event;
using Domain.ValueObject;
using Domain.Service.Evaluator;

namespace Domain.Service.Dealer;

public abstract class BaseTradeDealer : IDealer
{
    public void Start(
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        var players = GetPlayersForTrading(table);

        if (HasEnoughPlayersForTrading(players))
        {
            var startEvent = new StageIsStartedEvent(HandUid: handUid, OccuredAt: DateTime.Now);
            eventBus.Publish(startEvent);

            var previousPlayer = GetPreviousPlayer(table);
            RequestActionOrFinish(
                previousPlayer: previousPlayer,
                table: table,
                pot: pot,
                handUid: handUid,
                eventBus: eventBus
            );
        }
        else
        {
            var skipEvent = new StageIsSkippedEvent(HandUid: handUid, OccuredAt: DateTime.Now);
            eventBus.Publish(skipEvent);
        }
    }

    private IList<Player> GetPlayersForTrading(BaseTable table)
    {
        return table.Players.Where(x => x.IsAvailableForTrading).ToList();
    }

    private bool HasEnoughPlayersForTrading(IList<Player> players)
    {
        return players.Count > 1;
    }

    protected abstract Player? GetPreviousPlayer(BaseTable table);

    private void RequestActionOrFinish(
        Player? previousPlayer,
        BaseTable table,
        BasePot pot,
        HandUid handUid,
        EventBus eventBus
    )
    {
        var nextPlayer = table.GetNextPlayerForTrading(previousPlayer);
        if (nextPlayer == null || !pot.ActionIsAvailable(nextPlayer))
        {
            var finishEvent = new StageIsFinishedEvent(HandUid: handUid, OccuredAt: DateTime.Now);
            eventBus.Publish(finishEvent);
            return;
        }

        var foldIsAvailable = pot.FoldIsAvailable(nextPlayer);

        var checkIsAvailable = pot.CheckIsAvailable(nextPlayer);

        var callIsAvailable = pot.CallIsAvailable(nextPlayer);
        Chips? callToAmount = callIsAvailable ? pot.GetCallToAmount(nextPlayer) : null;

        var raiseIsAvailable = pot.RaiseIsAvailable(nextPlayer);
        Chips? minRaiseToAmount = raiseIsAvailable ? pot.GetMinRaiseToAmount(nextPlayer) : null;
        Chips? maxRaiseToAmount = raiseIsAvailable ? pot.GetMaxRaiseToAmount(nextPlayer) : null;

        var @event = new ActionIsRequestedEvent(
            Nickname: nextPlayer.Nickname,
            FoldIsAvailable: foldIsAvailable,
            CheckIsAvailable: checkIsAvailable,
            CallIsAvailable: callIsAvailable,
            CallToAmount: callToAmount,
            RaiseIsAvailable: raiseIsAvailable,
            MinRaiseToAmount: minRaiseToAmount,
            MaxRaiseToAmount: maxRaiseToAmount,
            HandUid: handUid,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(@event);
    }

    public void Fold(
        Nickname nickname,
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        var player = table.GetPlayerByNickname(nickname);
        pot.Fold(player);

        var @event = new PlayerFoldedEvent(
            Nickname: nickname,
            HandUid: handUid,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(@event);

        RequestActionOrFinish(
            previousPlayer: player,
            table: table,
            pot: pot,
            handUid: handUid,
            eventBus: eventBus
        );
    }

    public void Check(
        Nickname nickname,
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        var player = table.GetPlayerByNickname(nickname);
        pot.Check(player);

        var @event = new PlayerCheckedEvent(
            Nickname: nickname,
            HandUid: handUid,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(@event);

        RequestActionOrFinish(
            previousPlayer: player,
            table: table,
            pot: pot,
            handUid: handUid,
            eventBus: eventBus
        );
    }

    public void CallTo(
        Nickname nickname,
        Chips amount,
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        var player = table.GetPlayerByNickname(nickname);
        pot.CallTo(player, amount);

        var @event = new PlayerCalledToEvent(
            Nickname: nickname,
            Amount: amount,
            HandUid: handUid,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(@event);

        RequestActionOrFinish(
            previousPlayer: player,
            table: table,
            pot: pot,
            handUid: handUid,
            eventBus: eventBus
        );
    }

    public void RaiseTo(
        Nickname nickname,
        Chips amount,
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        var player = table.GetPlayerByNickname(nickname);
        pot.RaiseTo(player, amount);

        var @event = new PlayerRaisedToEvent(
            Nickname: nickname,
            Amount: amount,
            HandUid: handUid,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(@event);

        RequestActionOrFinish(
            previousPlayer: player,
            table: table,
            pot: pot,
            handUid: handUid,
            eventBus: eventBus
        );
    }
}

public class PreflopTradeDealer : BaseTradeDealer
{
    protected override Player? GetPreviousPlayer(BaseTable table)
    {
        return table.GetPlayerByPosition(Position.BigBlind);
    }
}

public class PostflopTradeDealer : BaseTradeDealer
{
    protected override Player? GetPreviousPlayer(BaseTable table)
    {
        return null;
    }
}