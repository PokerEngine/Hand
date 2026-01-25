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
            case PlayerActionType.Call:
                Call(player, pot);
                break;
            case PlayerActionType.RaiseTo:
                RaiseTo(player, action.Amount, pot);
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
        player.Post(new Chips(0));
        pot.PostBet(player.Nickname, new Chips(0));
    }

    private void Call(Player player, Pot pot)
    {
        var (isValid, reason) = CallIsValid(player, pot);
        if (!isValid)
        {
            throw new InvalidOperationException($"The player cannot call: {reason}");
        }

        var remainingAmount = GetCallAmount(player, pot) - pot.GetCurrentAmountPostedBy(player.Nickname);
        player.Post(remainingAmount);
        pot.PostBet(player.Nickname, remainingAmount);
    }

    private void RaiseTo(Player player, Chips amount, Pot pot)
    {
        var (isValid, reason) = RaiseToIsValid(player, amount, pot);
        if (!isValid)
        {
            throw new InvalidOperationException($"The player cannot raise to {amount}: {reason}");
        }

        var remainingAmount = amount - pot.GetCurrentAmountPostedBy(player.Nickname);
        player.Post(remainingAmount);
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

        var (callIsAvailable, _) = CallIsValid(player, pot);
        var callAmount = callIsAvailable ? GetCallAmount(player, pot) : new Chips(0);

        var (raiseIsAvailable, _) = RaiseToIsValid(player, null, pot);
        var minRaiseToAmount = raiseIsAvailable ? GetMinRaiseToAmount(player, pot) : new Chips(0);
        var maxRaiseToAmount = raiseIsAvailable ? GetMaxRaiseToAmount(player, pot) : new Chips(0);

        var @event = new PlayerActionRequestedEvent
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

    private (bool, string) CallIsValid(Player player, Pot pot)
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

        if (pot.PostedCurrentBet(player.Nickname) && !WasRaiseSincePlayerLastAction(player, pot))
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

    private Chips GetCallAmount(Player player, Pot pot)
    {
        var callAmount = pot.GetCurrentMaxAmountPostedNotBy(player.Nickname);
        var playerTotalAmount = pot.GetCurrentAmountPostedBy(player.Nickname) + player.Stack;
        return playerTotalAmount < callAmount ? playerTotalAmount : callAmount;
    }

    private Chips GetMinRaiseToAmount(Player player, Pot pot)
    {
        var minRaiseToAmount = pot.GetCurrentMaxAmountPostedNotBy(player.Nickname) + pot.LastRaisedStep;
        var playerTotalAmount = pot.GetCurrentAmountPostedBy(player.Nickname) + player.Stack;
        return playerTotalAmount < minRaiseToAmount ? playerTotalAmount : minRaiseToAmount;
    }

    protected abstract Chips GetMaxRaiseToAmount(Player player, Pot pot);
}

public class NoLimitBettingDealer : BaseBettingDealer
{
    protected override Chips GetMaxRaiseToAmount(Player player, Pot pot)
    {
        var playerTotalAmount = pot.GetCurrentAmountPostedBy(player.Nickname) + player.Stack;
        return playerTotalAmount;
    }
}

public class PotLimitBettingDealer : BaseBettingDealer
{
    protected override Chips GetMaxRaiseToAmount(Player player, Pot pot)
    {
        var playerPostedAmount = pot.GetCurrentAmountPostedBy(player.Nickname);
        var otherMaxPostedAmount = pot.GetCurrentMaxAmountPostedNotBy(player.Nickname);
        var potTotalAmountAfterCall = pot.TotalAmount + otherMaxPostedAmount - playerPostedAmount;
        var maxRaiseToAmount = otherMaxPostedAmount + potTotalAmountAfterCall;

        var playerTotalAmount = playerPostedAmount + player.Stack;
        return maxRaiseToAmount < playerTotalAmount ? maxRaiseToAmount : playerTotalAmount;
    }
}
