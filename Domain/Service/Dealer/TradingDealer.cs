using Domain.Entity;
using Domain.Error;
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
        var startEvent = new StageIsStartedEvent(HandUid: handUid, OccuredAt: DateTime.Now);
        eventBus.Publish(startEvent);

        if (HasEnoughPlayersForTrading(table))
        {
            var previousPlayer = GetPreviousPlayer(table: table, pot: pot);
            RequestDecisionOrFinish(
                previousPlayer: previousPlayer,
                table: table,
                pot: pot,
                handUid: handUid,
                eventBus: eventBus
            );
        }
        else
        {
            var finishEvent = new StageIsFinishedEvent(HandUid: handUid, OccuredAt: DateTime.Now);
            eventBus.Publish(finishEvent);
        }
    }

    public void CommitDecision(
        Nickname nickname,
        Decision decision,
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        switch (decision.Type)
        {
            case DecisionType.Fold:
                Fold(
                    nickname: nickname,
                    handUid: handUid,
                    table: table,
                    pot: pot,
                    deck: deck,
                    evaluator: evaluator,
                    eventBus: eventBus
                );
                break;
            case DecisionType.Check:
                Check(
                    nickname: nickname,
                    handUid: handUid,
                    table: table,
                    pot: pot,
                    deck: deck,
                    evaluator: evaluator,
                    eventBus: eventBus
                );
                break;
            case DecisionType.CallTo:
                CallTo(
                    nickname: nickname,
                    amount: decision.Amount,
                    handUid: handUid,
                    table: table,
                    pot: pot,
                    deck: deck,
                    evaluator: evaluator,
                    eventBus: eventBus
                );
                break;
            case DecisionType.RaiseTo:
                RaiseTo(
                    nickname: nickname,
                    amount: decision.Amount,
                    handUid: handUid,
                    table: table,
                    pot: pot,
                    deck: deck,
                    evaluator: evaluator,
                    eventBus: eventBus
                );
                break;
            default:
                throw new NotValidError("The decision is unknown");
        }
    }

    private bool HasEnoughPlayersForTrading(BaseTable table)
    {
        return GetPlayersForTrading(table).Count > 1;
    }

    private IList<Player> GetPlayersForTrading(BaseTable table)
    {
        return table.Where(x => x.IsAvailableForTrading).ToList();
    }

    private Player? GetPreviousPlayer(BaseTable table, BasePot pot)
    {
        if (pot.LastDecisionNickname == null)
        {
            return null;
        }
        return table.GetPlayerByNickname((Nickname)pot.LastDecisionNickname);
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

    private void RequestDecisionOrFinish(
        Player? previousPlayer,
        BaseTable table,
        BasePot pot,
        HandUid handUid,
        EventBus eventBus
    )
    {
        var nextPlayer = GetNextPlayerForTrading(table: table, previousPlayer: previousPlayer);
        if (nextPlayer == null || !pot.DecisionIsAvailable(nextPlayer))
        {
            Finish(
                pot: pot,
                handUid: handUid,
                eventBus: eventBus
            );
        }
        else
        {
            RequestDecision(
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

    private void RequestDecision(
        Player nextPlayer,
        BasePot pot,
        HandUid handUid,
        EventBus eventBus
    )
    {
        var foldIsAvailable = pot.FoldIsAvailable(nextPlayer);

        var checkIsAvailable = pot.CheckIsAvailable(nextPlayer);

        var callIsAvailable = pot.CallIsAvailable(nextPlayer);
        Chips callToAmount = callIsAvailable ? pot.GetCallToAmount(nextPlayer) : new Chips(0);

        var raiseIsAvailable = pot.RaiseIsAvailable(nextPlayer);
        Chips minRaiseToAmount = raiseIsAvailable ? pot.GetMinRaiseToAmount(nextPlayer) : new Chips(0);
        Chips maxRaiseToAmount = raiseIsAvailable ? pot.GetMaxRaiseToAmount(nextPlayer) : new Chips(0);

        var @event = new DecisionIsRequestedEvent(
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

    private void Fold(
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

        RequestDecisionOrFinish(
            previousPlayer: player,
            table: table,
            pot: pot,
            handUid: handUid,
            eventBus: eventBus
        );
    }

    private void Check(
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

        RequestDecisionOrFinish(
            previousPlayer: player,
            table: table,
            pot: pot,
            handUid: handUid,
            eventBus: eventBus
        );
    }

    private void CallTo(
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

        RequestDecisionOrFinish(
            previousPlayer: player,
            table: table,
            pot: pot,
            handUid: handUid,
            eventBus: eventBus
        );
    }

    private void RaiseTo(
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

        RequestDecisionOrFinish(
            previousPlayer: player,
            table: table,
            pot: pot,
            handUid: handUid,
            eventBus: eventBus
        );
    }
}
