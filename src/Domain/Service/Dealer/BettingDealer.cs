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
        var startEvent = new StageStartedEvent
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
            foreach (var @event in RequestPlayerActionOrFinish(table, pot))
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
            case PlayerActionRequestedEvent:
                break;
            case PlayerActedEvent e:
                SubmitPlayerAction(table.GetPlayerByNickname(e.Nickname), e.Action, pot);
                break;
            case BetRefundedEvent e:
                pot.RefundBet(e.Nickname, e.Amount);
                table.GetPlayerByNickname(e.Nickname).Refund(e.Amount);
                break;
            case BetsCollectedEvent:
                pot.CollectBets();
                break;
            case StageStartedEvent:
                break;
            case StageFinishedEvent:
                pot.ResetCurrentActions();
                break;
            default:
                throw new InvalidOperationException($"{@event.GetType().Name} is not supported");
        }
    }

    public IEnumerable<IEvent> SubmitPlayerAction(
        Nickname nickname,
        PlayerAction action,
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
            throw new InvalidOperationException("The player cannot act: it's not his turn");
        }

        var player = table.GetPlayerByNickname(nickname);
        SubmitPlayerAction(player, action, pot);

        var @event = new PlayerActedEvent
        {
            Nickname = nickname,
            Action = action,
            OccurredAt = DateTime.Now
        };
        yield return @event;

        foreach (var e in RequestPlayerActionOrFinish(table, pot))
        {
            yield return e;
        }
    }

    private void SubmitPlayerAction(Player player, PlayerAction action, Pot pot)
    {
        switch (action.Type)
        {
            case PlayerActionType.Fold:
                Fold(player);
                break;
            case PlayerActionType.Check:
                Check(player, pot);
                break;
            case PlayerActionType.CallBy:
                CallBy(player, action.Amount, pot);
                break;
            case PlayerActionType.RaiseBy:
                RaiseBy(player, action.Amount, pot);
                break;
            default:
                throw new InvalidOperationException($"{action} is not supported");
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

        // We post zero chips for check to mark that the player has committed his action
        player.Post(Chips.Zero);
        pot.PostBet(player.Nickname, Chips.Zero);
    }

    private void CallBy(Player player, Chips amount, Pot pot)
    {
        var (isValid, reason) = CallByIsValid(player, amount, pot);
        if (!isValid)
        {
            throw new InvalidOperationException($"The player cannot call by {amount}: {reason}");
        }

        player.Post(amount);
        pot.PostBet(player.Nickname, amount);
    }

    private void RaiseBy(Player player, Chips amount, Pot pot)
    {
        var (isValid, reason) = RaiseByIsValid(player, amount, pot);
        if (!isValid)
        {
            throw new InvalidOperationException($"The player cannot raise by {amount}: {reason}");
        }

        player.Post(amount);
        pot.PostBet(player.Nickname, amount);
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
        return table.GetPlayerNextToSeat(table.Positions.Button, IsAvailable)!;
    }

    private bool IsAvailable(Player player)
    {
        return !player.IsFolded && !player.IsAllIn;
    }

    private IEnumerable<IEvent> RequestPlayerActionOrFinish(Table table, Pot pot)
    {
        var previousPlayer = GetPreviousPlayer(table, pot);
        var nextPlayer = previousPlayer is null ? GetStartPlayer(table) : GetNextPlayer(table, previousPlayer);

        if (!EnoughPlayers(table) || nextPlayer is null || !PlayerActionIsExpected(nextPlayer, pot))
        {
            foreach (var @event in Finish(table, pot))
            {
                yield return @event;
            }
        }
        else
        {
            yield return RequestPlayerAction(nextPlayer, pot);
        }
    }

    private IEnumerable<IEvent> Finish(Table table, Pot pot)
    {
        var refundEvent = RefundBet(table, pot);
        if (refundEvent is not null)
        {
            yield return refundEvent;
        }

        if (pot.HasCurrentBets())
        {
            pot.CollectBets();

            yield return new BetsCollectedEvent
            {
                OccurredAt = DateTime.Now
            };
        }

        pot.ResetCurrentActions();

        yield return new StageFinishedEvent
        {
            OccurredAt = DateTime.Now
        };
    }

    private BetRefundedEvent? RefundBet(Table table, Pot pot)
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

        return new BetRefundedEvent
        {
            Nickname = nickname,
            Amount = amount,
            OccurredAt = DateTime.Now
        };
    }

    private PlayerActionRequestedEvent RequestPlayerAction(Player player, Pot pot)
    {
        var (checkIsAvailable, _) = CheckIsValid(player, pot);

        var (callIsAvailable, _) = CallByIsValid(player, null, pot);
        var callByAmount = callIsAvailable ? GetCallByAmount(player, pot) : Chips.Zero;

        var (raiseIsAvailable, _) = RaiseByIsValid(player, null, pot);
        var minRaiseByAmount = raiseIsAvailable ? GetMinRaiseByAmount(player, pot) : Chips.Zero;
        var maxRaiseByAmount = raiseIsAvailable ? GetMaxRaiseByAmount(player, pot) : Chips.Zero;

        var @event = new PlayerActionRequestedEvent
        {
            Nickname = player.Nickname,
            FoldIsAvailable = true,
            CheckIsAvailable = checkIsAvailable,
            CallIsAvailable = callIsAvailable,
            CallByAmount = callByAmount,
            RaiseIsAvailable = raiseIsAvailable,
            MinRaiseByAmount = minRaiseByAmount,
            MaxRaiseByAmount = maxRaiseByAmount,
            OccurredAt = DateTime.Now
        };
        return @event;
    }

    private bool PlayerActionIsExpected(Player player, Pot pot)
    {
        var playerPostedAmount = pot.GetCurrentAmountPostedBy(player.Nickname);
        var otherMaxPostedAmount = pot.GetCurrentMaxAmountPostedNotBy(player.Nickname);

        return playerPostedAmount < otherMaxPostedAmount || !pot.PostedCurrentBet(player.Nickname);
    }

    private (bool, string) CheckIsValid(Player player, Pot pot)
    {
        var playerPostedAmount = pot.GetCurrentAmountPostedBy(player.Nickname);
        var otherMaxPostedAmount = pot.GetCurrentMaxAmountPostedNotBy(player.Nickname);

        if (playerPostedAmount < otherMaxPostedAmount)
        {
            return (false, "There is a bet to call");
        }

        return (true, string.Empty);
    }

    private (bool, string) CallByIsValid(Player player, Chips? amount, Pot pot)
    {
        var playerPostedAmount = pot.GetCurrentAmountPostedBy(player.Nickname);
        var otherMaxPostedAmount = pot.GetCurrentMaxAmountPostedNotBy(player.Nickname);

        if (!otherMaxPostedAmount)
        {
            return (false, "There is no bet to call");
        }

        if (playerPostedAmount == otherMaxPostedAmount)
        {
            return (false, "There is no raise to call");
        }

        if (amount is not null)
        {
            var callByAmount = GetCallByAmount(player, pot);
            if (amount != callByAmount)
            {
                return (false, $"Should post {callByAmount}");
            }
        }

        return (true, string.Empty);
    }

    private (bool, string) RaiseByIsValid(Player player, Chips? amount, Pot pot)
    {
        var callByAmount = GetCallByAmount(player, pot);
        var minRaiseByAmount = GetMinRaiseByAmount(player, pot);
        var maxRaiseByAmount = GetMaxRaiseByAmount(player, pot);

        if (minRaiseByAmount == callByAmount)
        {
            return (false, "Not enough stack");
        }

        if (pot.PostedCurrentBet(player.Nickname) && !WasRaiseSincePlayerLastAction(player, pot))
        {
            return (false, "There is no raise since the player's last action and the following all-in");
        }

        if (amount is not null)
        {
            if (amount < minRaiseByAmount)
            {
                return (false, $"Minimum is {minRaiseByAmount}");
            }

            if (amount > maxRaiseByAmount)
            {
                return (false, $"Maximum is {maxRaiseByAmount}");
            }
        }

        return (true, string.Empty);
    }

    private bool WasRaiseSincePlayerLastAction(Player player, Pot pot)
    {
        var playerPostedAmount = pot.GetCurrentAmountPostedBy(player.Nickname);
        var otherMaxPostedAmount = pot.GetCurrentMaxAmountPostedNotBy(player.Nickname);

        // No one has posted more than the player -> no raise since their last action
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

    private Chips GetCallByAmount(Player player, Pot pot)
    {
        var otherMaxPostedAmount = pot.GetCurrentMaxAmountPostedNotBy(player.Nickname);
        var playerPostedAmount = pot.GetCurrentAmountPostedBy(player.Nickname);
        var callByAmount = otherMaxPostedAmount - playerPostedAmount;
        return callByAmount < player.Stack ? callByAmount : player.Stack;
    }

    private Chips GetMinRaiseByAmount(Player player, Pot pot)
    {
        var otherMaxPostedAmount = pot.GetCurrentMaxAmountPostedNotBy(player.Nickname);
        var playerPostedAmount = pot.GetCurrentAmountPostedBy(player.Nickname);
        var minRaiseByAmount = otherMaxPostedAmount - playerPostedAmount + pot.LastRaisedStep;
        return minRaiseByAmount < player.Stack ? minRaiseByAmount : player.Stack;
    }

    protected abstract Chips GetMaxRaiseByAmount(Player player, Pot pot);
}

public class NoLimitBettingDealer : BaseBettingDealer
{
    protected override Chips GetMaxRaiseByAmount(Player player, Pot pot)
    {
        return player.Stack;
    }
}

public class PotLimitBettingDealer : BaseBettingDealer
{
    protected override Chips GetMaxRaiseByAmount(Player player, Pot pot)
    {
        var otherMaxPostedAmount = pot.GetCurrentMaxAmountPostedNotBy(player.Nickname);
        var playerPostedAmount = pot.GetCurrentAmountPostedBy(player.Nickname);
        var potTotalAmountAfterCall = pot.TotalAmount + otherMaxPostedAmount - playerPostedAmount;
        var maxRaiseByAmount = otherMaxPostedAmount + potTotalAmountAfterCall - playerPostedAmount;
        return maxRaiseByAmount < player.Stack ? maxRaiseByAmount : player.Stack;
    }
}
