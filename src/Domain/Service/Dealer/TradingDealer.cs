using Domain.Entity;
using Domain.Event;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Domain.Service.Dealer;

public class TradingDealer : IDealer
{
    public IEnumerable<IEvent> Start(
        Game game,
        Table table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        var startEvent = new StageIsStartedEvent
        {
            OccurredAt = DateTime.Now
        };
        yield return startEvent;

        var players = GetPlayersForTrading(table);
        if (HasEnoughPlayersForTrading(players))
        {
            var previousPlayer = GetPreviousPlayer(table, pot);
            yield return RequestDecisionOrFinish(players, previousPlayer, pot);
        }
        else
        {
            var finishEvent = new StageIsFinishedEvent
            {
                OccurredAt = DateTime.Now
            };
            yield return finishEvent;
        }
    }

    public void Handle(
        IEvent @event,
        Game game,
        Table table,
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
                throw new InvalidOperationException($"{@event.GetType().Name} is not supported");
        }
    }

    public IEnumerable<IEvent> CommitDecision(
        Nickname nickname,
        Decision decision,
        Game game,
        Table table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
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

        var @event = new DecisionIsCommittedEvent
        {
            Nickname = nickname,
            Decision = decision,
            OccurredAt = DateTime.Now
        };
        yield return @event;

        yield return RequestDecisionOrFinish(players, player, pot);
    }

    private List<Player> GetPlayersForTrading(Table table)
    {
        var startSeat = table.IsHeadsUp() ? table.BigBlindSeat : table.SmallBlindSeat;
        return table.GetPlayersStartingFromSeat(startSeat).Where(x => !x.IsFolded && !x.IsAllIn).ToList();
    }

    private bool HasEnoughPlayersForTrading(List<Player> players)
    {
        return players.Count > 1;
    }

    private Player? GetPreviousPlayer(Table table, BasePot pot)
    {
        if (pot.LastDecisionNickname == null)
        {
            return null;
        }
        return table.GetPlayerByNickname((Nickname)pot.LastDecisionNickname);
    }

    private Player? GetNextPlayerForTrading(List<Player> players, Player? previousPlayer)
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

    private IEvent RequestDecisionOrFinish(
        List<Player> players,
        Player? previousPlayer,
        BasePot pot
    )
    {
        var nextPlayer = GetNextPlayerForTrading(players, previousPlayer);
        if (nextPlayer == null || !pot.DecisionIsAvailable(nextPlayer))
        {

            return Finish(pot);
        }
        else
        {
            return RequestDecision(nextPlayer, pot);
        }
    }

    private StageIsFinishedEvent Finish(BasePot pot)
    {
        pot.FinishStage();

        var @event = new StageIsFinishedEvent
        {
            OccurredAt = DateTime.Now
        };
        return @event;
    }

    private DecisionIsRequestedEvent RequestDecision(Player player, BasePot pot)
    {
        var foldIsAvailable = pot.FoldIsAvailable(player);

        var checkIsAvailable = pot.CheckIsAvailable(player);

        var callIsAvailable = pot.CallIsAvailable(player);
        Chips callToAmount = callIsAvailable ? pot.GetCallToAmount(player) : new Chips(0);

        var raiseIsAvailable = pot.RaiseIsAvailable(player);
        Chips minRaiseToAmount = raiseIsAvailable ? pot.GetMinRaiseToAmount(player) : new Chips(0);
        Chips maxRaiseToAmount = raiseIsAvailable ? pot.GetMaxRaiseToAmount(player) : new Chips(0);

        var @event = new DecisionIsRequestedEvent
        {
            Nickname = player.Nickname,
            FoldIsAvailable = foldIsAvailable,
            CheckIsAvailable = checkIsAvailable,
            CallIsAvailable = callIsAvailable,
            CallToAmount = callToAmount,
            RaiseIsAvailable = raiseIsAvailable,
            MinRaiseToAmount = minRaiseToAmount,
            MaxRaiseToAmount = maxRaiseToAmount,
            OccurredAt = DateTime.Now
        };
        return @event;
    }
}
