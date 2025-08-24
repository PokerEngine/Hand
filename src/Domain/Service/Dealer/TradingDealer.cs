using Domain.Entity;
using Domain.Event;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Domain.Service.Dealer;

public class TradingDealer : IDealer
{
    public void Start(
        Game game,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator,
        IEventBus eventBus
    )
    {
        var startEvent = new StageIsStartedEvent(OccuredAt: DateTime.Now);
        eventBus.Publish(startEvent);

        var players = GetPlayersForTrading(table);
        if (HasEnoughPlayersForTrading(players))
        {
            var previousPlayer = GetPreviousPlayer(table, pot);
            RequestDecisionOrFinish(
                previousPlayer: previousPlayer,
                players: players,
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
        BaseEvent @event,
        Game game,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
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
                throw new ArgumentException("The event is not supported", nameof(@event));
        }
    }

    public void CommitDecision(
        Nickname nickname,
        Decision decision,
        Game game,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator,
        IEventBus eventBus
    )
    {
        var players = GetPlayersForTrading(table);
        var previousPlayer = GetPreviousPlayer(table, pot);
        var expectedPlayer = GetNextPlayerForTrading(players, previousPlayer);
        if (expectedPlayer == null || expectedPlayer.Nickname != nickname)
        {
            throw new InvalidOperationException("The player cannot commit a decision for now");
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
            players: players,
            previousPlayer: player,
            pot: pot,
            eventBus: eventBus
        );
    }

    private IList<Player> GetPlayersForTrading(BaseTable table)
    {
        var startSeat = table.IsHeadsUp() ? table.BigBlindSeat : table.SmallBlindSeat;
        return table.GetPlayersStartingFromSeat(startSeat).Where(x => !x.IsFolded && !x.IsAllIn).ToList();
    }

    private bool HasEnoughPlayersForTrading(IList<Player> players)
    {
        return players.Count > 1;
    }

    private Player? GetPreviousPlayer(BaseTable table, BasePot pot)
    {
        if (pot.LastDecisionNickname == null)
        {
            return null;
        }
        return table.GetPlayerByNickname((Nickname)pot.LastDecisionNickname);
    }

    private Player? GetNextPlayerForTrading(IList<Player> players, Player? previousPlayer)
    {
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
        IList<Player> players,
        Player? previousPlayer,
        BasePot pot,
        IEventBus eventBus
    )
    {
        var nextPlayer = GetNextPlayerForTrading(players, previousPlayer);
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
