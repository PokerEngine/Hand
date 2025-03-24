using Domain.Entity;
using Domain.Event;
using Domain.ValueObject;
using Domain.Service.Evaluator;

namespace Domain.Service.Dealer;

public class TradingDealer : IDealer
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
        if (HasEnoughPlayersForTrading(table))
        {
            var startEvent = new StageIsStartedEvent(HandUid: handUid, OccuredAt: DateTime.Now);
            eventBus.Publish(startEvent);

            var previousPlayer = GetPreviousPlayer(table: table, pot: pot);
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

    private bool HasEnoughPlayersForTrading(BaseTable table)
    {
        return GetPlayersForTrading(table).Count > 1;
    }

    private IList<Player> GetPlayersForTrading(BaseTable table)
    {
        return table.Players.Where(x => x.IsAvailableForTrading).ToList();
    }

    private Player? GetPreviousPlayer(BaseTable table, BasePot pot)
    {
        if (pot.LastActionNickname == null)
        {
            return null;
        }
        return table.GetPlayerByNickname((Nickname)pot.LastActionNickname);
    }

    private Player? GetNextPlayerForTrading(BaseTable table, Player? previousPlayer)
    {
        var players = GetPlayersForTrading(table);
        var previousIdx = previousPlayer == null ? -1 : players.IndexOf(previousPlayer);
        var nextIdx = previousIdx + 1;

        while (true)
        {
            if (nextIdx == previousIdx)
            {
                break;
            }

            if (nextIdx == players.Count)
            {
                if (previousIdx == -1)
                {
                    break;
                }

                nextIdx = 0;
            }

            var nextPlayer = players[nextIdx];
            if (nextPlayer.IsAvailableForTrading)
            {
                return nextPlayer;
            }

            nextIdx ++;
        }

        return null;
    }

    private void RequestActionOrFinish(
        Player? previousPlayer,
        BaseTable table,
        BasePot pot,
        HandUid handUid,
        EventBus eventBus
    )
    {
        var nextPlayer = GetNextPlayerForTrading(table: table, previousPlayer: previousPlayer);
        if (nextPlayer == null || !pot.ActionIsAvailable(nextPlayer))
        {
            Finish(
                pot: pot,
                handUid: handUid,
                eventBus: eventBus
            );
        }
        else
        {
            RequestAction(
                nextPlayer: nextPlayer,
                pot: pot,
                handUid: handUid,
                eventBus: eventBus
            );
        }
    }

    private void Finish(
        BasePot pot,
        HandUid handUid,
        EventBus eventBus
    )
    {
        pot.FinishStage();

        var finishEvent = new StageIsFinishedEvent(HandUid: handUid, OccuredAt: DateTime.Now);
        eventBus.Publish(finishEvent);
    }

    private void RequestAction(
        Player nextPlayer,
        BasePot pot,
        HandUid handUid,
        EventBus eventBus
    )
    {
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
