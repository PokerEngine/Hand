using Domain.Entity;
using Domain.Error;
using Domain.Event;
using Domain.ValueObject;
using Domain.Service.Evaluator;

namespace Domain.Service.Dealer;

public class TradingDealer : IDealer
{
    public void Start(
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        IEventBus eventBus
    )
    {
        var startEvent = new StageIsStartedEvent(OccuredAt: DateTime.Now);
        eventBus.Publish(startEvent);

        if (HasEnoughPlayersForTrading(table))
        {
            var previousPlayer = GetPreviousPlayer(table: table, pot: pot);
            RequestDecisionOrFinish(
                previousPlayer: previousPlayer,
                table: table,
                pot: pot,
                eventBus: eventBus
            );
        }
        else
        {
            var finishEvent = new StageIsFinishedEvent(OccuredAt: DateTime.Now);
            eventBus.Publish(finishEvent);
        }
    }

    public void Handle(
        IEvent @event,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator
    )
    {
        switch (@event)
        {
            case DecisionIsRequestedEvent:
                break;
            case DecisionIsCommittedEvent e:
                pot.CommitDecision(table.GetPlayerByNickname(e.Nickname), e.Decision);
                break;
            case StageIsStartedEvent:
                break;
            case StageIsFinishedEvent:
                pot.FinishStage();
                break;
            default:
                throw new NotAvailableError($"The event {@event} is not supported");
        }
    }

    public void CommitDecision(
        Nickname nickname,
        Decision decision,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        IEventBus eventBus
    )
    {
        var previousPlayer = GetPreviousPlayer(table: table, pot: pot);
        var expectedPlayer = GetNextPlayerForTrading(table: table, previousPlayer: previousPlayer);
        if (expectedPlayer == null || expectedPlayer.Nickname != nickname)
        {
            throw new NotAvailableError("The player cannot commit a decision for now");
        }

        var player = table.GetPlayerByNickname(nickname);
        pot.CommitDecision(player, decision);

        var @event = new DecisionIsCommittedEvent(
            Nickname: nickname,
            Decision: decision,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(@event);

        RequestDecisionOrFinish(
            previousPlayer: player,
            table: table,
            pot: pot,
            eventBus: eventBus
        );
    }

    private bool HasEnoughPlayersForTrading(BaseTable table)
    {
        return GetPlayersForTrading(table).Count > 1;
    }

    private IList<Player> GetPlayersForTrading(BaseTable table)
    {
        return table.Where(x => !x.IsFolded && !x.IsAllIn).ToList();
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

            return players[nextIdx];
        }

        return null;
    }

    private void RequestDecisionOrFinish(
        Player? previousPlayer,
        BaseTable table,
        BasePot pot,
        IEventBus eventBus
    )
    {
        var nextPlayer = GetNextPlayerForTrading(table: table, previousPlayer: previousPlayer);
        if (nextPlayer == null || !pot.DecisionIsAvailable(nextPlayer))
        {
            Finish(
                pot: pot,
                eventBus: eventBus
            );
        }
        else
        {
            RequestDecision(
                nextPlayer: nextPlayer,
                pot: pot,
                eventBus: eventBus
            );
        }
    }

    private void Finish(
        BasePot pot,
        IEventBus eventBus
    )
    {
        pot.FinishStage();

        var finishEvent = new StageIsFinishedEvent(OccuredAt: DateTime.Now);
        eventBus.Publish(finishEvent);
    }

    private void RequestDecision(
        Player nextPlayer,
        BasePot pot,
        IEventBus eventBus
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
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(@event);
    }
}
