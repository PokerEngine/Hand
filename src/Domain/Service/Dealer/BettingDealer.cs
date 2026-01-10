using Domain.Entity;
using Domain.Event;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Domain.Service.Dealer;

public abstract class BaseBettingDealer : IDealer
{
    public IEnumerable<IEvent> Start(
        Rules rules,
        Table table,
        Pot pot,
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

        if (!EnoughPlayers(table))
        {
            foreach (var @event in Finish(table, pot))
            {
                yield return @event;
            }
        }
        else
        {
            foreach (var @event in RequestDecisionOrFinish(table, pot))
            {
                yield return @event;
            }
        }
    }

    public void Handle(
        IEvent @event,
        Rules rules,
        Table table,
        Pot pot,
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
                CommitDecision(table.GetPlayerByNickname(e.Nickname), e.Decision, pot);
                break;
            case RefundIsCommittedEvent e:
                pot.RefundBet(e.Nickname, e.Amount);
                table.GetPlayerByNickname(e.Nickname).Refund(e.Amount);
                break;
            case StageIsStartedEvent:
                break;
            case StageIsFinishedEvent:
                pot.CommitBets();
                break;
            default:
                throw new InvalidOperationException($"{@event.GetType().Name} is not supported");
        }
    }

    public IEnumerable<IEvent> CommitDecision(
        Nickname nickname,
        Decision decision,
        Rules rules,
        Table table,
        Pot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        var previousPlayer = GetPreviousPlayer(table, pot);
        var nextPlayer = previousPlayer is null ? GetStartPlayer(table) : GetNextPlayer(table, previousPlayer);
        if (nextPlayer is null || nextPlayer.Nickname != nickname)
        {
            throw new InvalidOperationException("The player cannot commit a decision for now");
        }

        var player = table.GetPlayerByNickname(nickname);
        CommitDecision(player, decision, pot);

        var @event = new DecisionIsCommittedEvent
        {
            Nickname = nickname,
            Decision = decision,
            OccurredAt = DateTime.Now
        };
        yield return @event;

        foreach (var e in RequestDecisionOrFinish(table, pot))
        {
            yield return e;
        }
    }

    private void CommitDecision(Player player, Decision decision, Pot pot)
    {
        switch (decision.Type)
        {
            case DecisionType.Fold:
                Fold(player);
                break;
            case DecisionType.Check:
                Check(player, pot);
                break;
            case DecisionType.Call:
                Call(player, pot);
                break;
            case DecisionType.RaiseTo:
                RaiseTo(player, decision.Amount, pot);
                break;
            default:
                throw new InvalidOperationException($"{decision} is not supported");
        }
    }

    private void Fold(Player player)
    {
        player.Fold();
    }

    private void Check(Player player, Pot pot)
    {
        var (isValid, reason) = CheckIsValid(player, pot);
        if (!isValid)
        {
            throw new InvalidOperationException($"The player cannot check: {reason}");
        }

        player.Check();

        // We post zero chips for check to mark that the player has committed his decision
        pot.PostBet(player.Nickname, new Chips(0));
    }

    private void Call(Player player, Pot pot)
    {
        var (isValid, reason) = CallIsValid(player, pot);
        if (!isValid)
        {
            throw new InvalidOperationException($"The player cannot call: {reason}");
        }

        var remainingAmount = GetCallAmount(player, pot) - pot.GetUncommittedAmountPostedBy(player.Nickname);
        player.Bet(remainingAmount);
        pot.PostBet(player.Nickname, remainingAmount);
    }

    private void RaiseTo(Player player, Chips amount, Pot pot)
    {
        var (isValid, reason) = RaiseToIsValid(player, amount, pot);
        if (!isValid)
        {
            throw new InvalidOperationException($"The player cannot raise to {amount}: {reason}");
        }

        var remainingAmount = amount - pot.GetUncommittedAmountPostedBy(player.Nickname);
        player.Bet(remainingAmount);
        pot.PostBet(player.Nickname, remainingAmount);
    }

    private bool EnoughPlayers(Table table)
    {
        return table.Players.Count(IsAvailable) > 1;
    }

    private Player? GetPreviousPlayer(Table table, Pot pot)
    {
        if (pot.LastPostedNickname is null)
        {
            return null;
        }

        return table.GetPlayerByNickname((Nickname)pot.LastPostedNickname);
    }

    private Player? GetNextPlayer(Table table, Player previousPlayer)
    {
        return table.GetPlayerNextToSeat(previousPlayer.Seat, IsAvailable);
    }

    private Player GetStartPlayer(Table table)
    {
        return table.GetPlayerNextToSeat(table.ButtonSeat, IsAvailable)!;
    }

    private bool IsAvailable(Player player)
    {
        return !player.IsFolded && !player.IsAllIn;
    }

    private IEnumerable<IEvent> RequestDecisionOrFinish(Table table, Pot pot)
    {
        var previousPlayer = GetPreviousPlayer(table, pot);
        var nextPlayer = previousPlayer is null ? GetStartPlayer(table) : GetNextPlayer(table, previousPlayer);

        if (nextPlayer is null || !DecisionIsExpected(nextPlayer, pot))
        {
            foreach (var @event in Finish(table, pot))
            {
                yield return @event;
            }
        }
        else
        {
            yield return RequestDecision(nextPlayer, pot);
        }
    }

    private IEnumerable<IEvent> Finish(Table table, Pot pot)
    {
        var refundEvent = Refund(table, pot);
        if (refundEvent is not null)
        {
            yield return refundEvent;
        }

        pot.CommitBets();

        yield return new StageIsFinishedEvent
        {
            OccurredAt = DateTime.Now
        };
    }

    private RefundIsCommittedEvent? Refund(Table table, Pot pot)
    {
        var (nn, amount) = pot.CalculateRefund();
        if (nn is null)
        {
            return null;
        }

        var nickname = (Nickname)nn;
        var player = table.GetPlayerByNickname(nickname);
        pot.RefundBet(nickname, amount);
        player.Refund(amount);

        return new RefundIsCommittedEvent
        {
            Nickname = nickname,
            Amount = amount,
            OccurredAt = DateTime.Now
        };
    }

    private DecisionIsRequestedEvent RequestDecision(Player player, Pot pot)
    {
        var (checkIsAvailable, _) = CheckIsValid(player, pot);

        var (callIsAvailable, _) = CallIsValid(player, pot);
        var callAmount = callIsAvailable ? GetCallAmount(player, pot) : new Chips(0);

        var (raiseIsAvailable, _) = RaiseToIsValid(player, null, pot);
        var minRaiseToAmount = raiseIsAvailable ? GetMinRaiseToAmount(player, pot) : new Chips(0);
        var maxRaiseToAmount = raiseIsAvailable ? GetMaxRaiseToAmount(player, pot) : new Chips(0);

        var @event = new DecisionIsRequestedEvent
        {
            Nickname = player.Nickname,
            FoldIsAvailable = true,
            CheckIsAvailable = checkIsAvailable,
            CallIsAvailable = callIsAvailable,
            CallToAmount = callAmount,
            RaiseIsAvailable = raiseIsAvailable,
            MinRaiseToAmount = minRaiseToAmount,
            MaxRaiseToAmount = maxRaiseToAmount,
            OccurredAt = DateTime.Now
        };
        return @event;
    }

    private bool DecisionIsExpected(Player player, Pot pot)
    {
        var playerPostedAmount = pot.GetUncommittedAmountPostedBy(player.Nickname);
        var otherMaxPostedAmount = pot.GetUncommittedMaxAmountPostedNotBy(player.Nickname);

        return playerPostedAmount < otherMaxPostedAmount || !pot.PostedUncommittedBet(player.Nickname);
    }

    private (bool, string) CheckIsValid(Player player, Pot pot)
    {
        var playerPostedAmount = pot.GetUncommittedAmountPostedBy(player.Nickname);
        var otherMaxPostedAmount = pot.GetUncommittedMaxAmountPostedNotBy(player.Nickname);

        if (playerPostedAmount < otherMaxPostedAmount)
        {
            return (false, "There is a bet to call");
        }

        return (true, string.Empty);
    }

    private (bool, string) CallIsValid(Player player, Pot pot)
    {
        var playerPostedAmount = pot.GetUncommittedAmountPostedBy(player.Nickname);
        var otherMaxPostedAmount = pot.GetUncommittedMaxAmountPostedNotBy(player.Nickname);

        if (!otherMaxPostedAmount)
        {
            return (false, "There is no bet to call");
        }

        if (playerPostedAmount == otherMaxPostedAmount)
        {
            return (false, "There is no raise to call");
        }

        return (true, string.Empty);
    }

    private (bool, string) RaiseToIsValid(Player player, Chips? amount, Pot pot)
    {
        var callAmount = GetCallAmount(player, pot);
        var minRaiseToAmount = GetMinRaiseToAmount(player, pot);
        var maxRaiseToAmount = GetMaxRaiseToAmount(player, pot);

        if (minRaiseToAmount == callAmount)
        {
            return (false, "Not enough stack");
        }

        if (pot.PostedUncommittedBet(player.Nickname) && !WasRaiseSincePlayerLastDecision(player, pot))
        {
            return (false, "There is no raise since the player's last action and the following all-in");
        }

        if (amount is not null)
        {
            if (amount < minRaiseToAmount)
            {
                return (false, $"Minimum is {minRaiseToAmount}");
            }

            if (amount > maxRaiseToAmount)
            {
                return (false, $"Maximum is {maxRaiseToAmount}");
            }
        }

        return (true, string.Empty);
    }

    private bool WasRaiseSincePlayerLastDecision(Player player, Pot pot)
    {
        var playerPostedAmount = pot.GetUncommittedAmountPostedBy(player.Nickname);
        var otherMaxPostedAmount = pot.GetUncommittedMaxAmountPostedNotBy(player.Nickname);

        // No one has posted more than the player -> no raise since their last decision
        if (playerPostedAmount >= otherMaxPostedAmount)
        {
            return false;
        }

        // There was either no real raise (only smaller all-ins), or the raiser was the player himself
        if (pot.LastRaisedNickname is null || pot.LastRaisedNickname == player.Nickname)
        {
            return false;
        }

        return true;
    }

    private Chips GetCallAmount(Player player, Pot pot)
    {
        var callAmount = pot.GetUncommittedMaxAmountPostedNotBy(player.Nickname);
        var playerTotalAmount = pot.GetUncommittedAmountPostedBy(player.Nickname) + player.Stack;
        return playerTotalAmount < callAmount ? playerTotalAmount : callAmount;
    }

    private Chips GetMinRaiseToAmount(Player player, Pot pot)
    {
        var minRaiseToAmount = pot.GetUncommittedMaxAmountPostedNotBy(player.Nickname) + pot.LastRaisedStep;
        var playerTotalAmount = pot.GetUncommittedAmountPostedBy(player.Nickname) + player.Stack;
        return playerTotalAmount < minRaiseToAmount ? playerTotalAmount : minRaiseToAmount;
    }

    protected abstract Chips GetMaxRaiseToAmount(Player player, Pot pot);
}

public class NoLimitBettingDealer : BaseBettingDealer
{
    protected override Chips GetMaxRaiseToAmount(Player player, Pot pot)
    {
        var playerTotalAmount = pot.GetUncommittedAmountPostedBy(player.Nickname) + player.Stack;
        return playerTotalAmount;
    }
}

public class PotLimitBettingDealer : BaseBettingDealer
{
    protected override Chips GetMaxRaiseToAmount(Player player, Pot pot)
    {
        var playerPostedAmount = pot.GetUncommittedAmountPostedBy(player.Nickname);
        var otherMaxPostedAmount = pot.GetUncommittedMaxAmountPostedNotBy(player.Nickname);
        var potTotalAmountAfterCall = pot.TotalAmount + otherMaxPostedAmount - playerPostedAmount;
        var maxRaiseToAmount = otherMaxPostedAmount + potTotalAmountAfterCall;

        var playerTotalAmount = playerPostedAmount + player.Stack;
        return maxRaiseToAmount < playerTotalAmount ? maxRaiseToAmount : playerTotalAmount;
    }
}
