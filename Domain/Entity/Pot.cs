using System.Collections.Immutable;

using Domain.Error;
using Domain.ValueObject;

namespace Domain.Entity;

public abstract class BasePot
{
    public Chips SmallBlind { get; }
    public Chips BigBlind { get; }
    public Nickname? LastDecisionNickname { get; private set; }

    private Nickname? _lastRaiseNickname;
    private Chips _lastRaiseStep;
    private HashSet<Nickname> _currentDecisionCommittedNicknames;
    private SidePot _currentSidePot;
    private SidePot _previousSidePot;

    protected BasePot(
        Chips smallBlind,
        Chips bigBlind,
        Nickname? lastDecisionNickname,
        Nickname? lastRaiseNickname,
        Chips lastRaiseStep,
        IEnumerable<Nickname> currentDecisionNicknames,
        SidePot currentSidePot,
        SidePot previousSidePot
    )
    {
        SmallBlind = smallBlind;
        BigBlind = bigBlind;
        LastDecisionNickname = lastDecisionNickname;

        _lastRaiseNickname = lastRaiseNickname;
        _lastRaiseStep = lastRaiseStep;
        _currentDecisionCommittedNicknames = currentDecisionNicknames.ToHashSet();
        _currentSidePot = currentSidePot;
        _previousSidePot = previousSidePot;
    }

    public void PostSmallBlind(Player player, Chips amount)
    {
        ValidatePostBlind(player: player, amount: amount, blind: SmallBlind);

        PostTo(player, amount);
        LastDecisionNickname = player.Nickname;
    }

    public void PostBigBlind(Player player, Chips amount)
    {
        ValidatePostBlind(player: player, amount: amount, blind: BigBlind);

        PostTo(player, amount);
        LastDecisionNickname = player.Nickname;
    }

    public void Fold(Player player)
    {
        ValidateFold(player);

        player.Fold();

        var amount = GetCurrentPostedAmount(player);
        if (amount)
        {
            _currentSidePot = _currentSidePot.Sub(player.Nickname, amount);
            _previousSidePot = _previousSidePot.Add(player.Nickname, amount);
        }

        LastDecisionNickname = player.Nickname;
    }

    public void Check(Player player)
    {
        ValidateCheck(player);

        player.Check();
        CommitCurrentDecision(player);
        LastDecisionNickname = player.Nickname;
    }

    public void CallTo(Player player, Chips amount)
    {
        ValidateCallTo(player, amount);

        PostTo(player, amount);
        CommitCurrentDecision(player);
        LastDecisionNickname = player.Nickname;
    }

    public void RaiseTo(Player player, Chips amount)
    {
        ValidateRaiseTo(player, amount);

        var raiseStep = new Chips(0);
        var currentMaxAmount = GetCurrentMaxAmount(player);
        if (amount >= currentMaxAmount + _lastRaiseStep)
        {
            raiseStep = amount - currentMaxAmount;
        }

        PostTo(player, amount);
        CommitCurrentDecision(player);
        LastDecisionNickname = player.Nickname;

        // If a player raises less than the minimum amount, he goes all in, and it is not considered a raise
        if (raiseStep)
        {
            _lastRaiseStep = raiseStep;
            _lastRaiseNickname = player.Nickname;
        }
    }

    public void Refund(Player player, Chips amount)
    {
        ValidateRefund(player, amount);

        _previousSidePot = _previousSidePot.Sub(player.Nickname, amount);
        player.Refund(amount);
    }

    public void WinWithoutShowdown(Player player, Chips amount)
    {
        ValidateWinWithoutShowdown(player, amount);

        _previousSidePot = new SidePot();
        _currentSidePot = new SidePot();

        player.Win(amount);
    }

    public void WinAtShowdown(IList<(Player, Combo, Chips)> playerComboAmounts)
    {
        ValidateWinAtShowdown(playerComboAmounts);

        _previousSidePot = new SidePot();
        _currentSidePot = new SidePot();

        foreach (var (player, _, amount) in playerComboAmounts)
        {
            player.Win(amount);
        }
    }

    public void FinishStage()
    {
        LastDecisionNickname = null;
        _lastRaiseStep = BigBlind;
        _lastRaiseNickname = null;
        _currentDecisionCommittedNicknames.Clear();

        foreach (var (nickname, amount) in _currentSidePot)
        {
            _currentSidePot = _currentSidePot.Sub(nickname, amount);
            _previousSidePot = _previousSidePot.Add(nickname, amount);
        }
    }

    public Chips GetTotalAmount()
    {
        return _currentSidePot.Amount + _previousSidePot.Amount;
    }

    public Chips GetCurrentAmount()
    {
        return _currentSidePot.Amount;
    }

    public Chips GetCurrentPostedAmount(Player player)
    {
        return _currentSidePot.Get(player.Nickname);
    }

    public Chips GetPreviousPostedAmount(Player player)
    {
        return _previousSidePot.Get(player.Nickname);
    }

    public Chips GetCallToAmount(Player player)
    {
        var playerAmount = GetCurrentPostedAmount(player) + player.Stake;
        var currentMaxAmount = GetCurrentMaxAmount(player);
        return currentMaxAmount < playerAmount ? currentMaxAmount : playerAmount;
    }

    public Chips GetMinRaiseToAmount(Player player)
    {
        var playerAmount = GetCurrentPostedAmount(player) + player.Stake;
        var amount = GetCurrentMaxAmount(player) + _lastRaiseStep;
        return amount < playerAmount ? amount : playerAmount;
    }

    public abstract Chips GetMaxRaiseToAmount(Player player);

    public Chips GetRefundAmount(Player player)
    {
        var postedAmount = GetPreviousPostedAmount(player);
        var amount = GetPreviousMaxAmount(player);
        return postedAmount > amount ? postedAmount - amount : new Chips(0);
    }

    public IList<(Player, Combo, Chips)> GetWinAtShowdownAmounts(IList<(Player, Combo)> playerCombos)
    {
        var weightMapping = new Dictionary<int, List<(Player, Combo)>>();
        var players = new List<Player>();

        foreach (var (player, combo) in playerCombos)
        {
            if (weightMapping.ContainsKey(combo.Weight))
            {
                weightMapping[combo.Weight].Add((player, combo));
            }
            else
            {
                weightMapping[combo.Weight] = [(player, combo)];
            }

            players.Add(player);
        }

        var sidePots = GetSidePots(players);
        var winMapping = playerCombos.ToDictionary(x => x, _ => new Chips(0));

        foreach (var weight in weightMapping.Keys.OrderByDescending(x => x))
        {
            var groupPlayerCombos = weightMapping[weight].OrderBy(x => (x.Item1.Stake, x.Item1.Nickname)).ToList();
            var groupNicknames = groupPlayerCombos.Select(x => x.Item1.Nickname).ToHashSet();

            var i = 0;
            while (i < sidePots.Count)
            {
                var commonNicknames = groupNicknames.Intersect(sidePots[i].Nicknames).ToHashSet();
                if (commonNicknames.Count == 0)
                {
                    i++;
                    continue;
                }

                var quotient = sidePots[i].Amount / commonNicknames.Count;
                var remainder = sidePots[i].Amount % commonNicknames.Count;

                foreach (var (player, combo) in groupPlayerCombos)
                {
                    if (!commonNicknames.Contains(player.Nickname))
                    {
                        continue;
                    }

                    var key = (player, combo);
                    winMapping[key] += quotient;

                    if (remainder)
                    {
                        // We give the remainder to the poorest player
                        winMapping[key] += remainder;
                        remainder = new Chips(0);
                    }
                }

                sidePots.RemoveAt(i);
            }
        }

        return winMapping.Select(x => (x.Key.Item1, x.Key.Item2, x.Value)).ToList();
    }

    public IList<SidePot> GetSidePots(IList<Player> players)
    {
        var nicknames = players.Select(x => x.Nickname).ToHashSet();
        var sidePots = new List<SidePot>();
        var totalPot = _previousSidePot.Merge(_currentSidePot);

        while (totalPot.Amount)
        {
            var minAmount = totalPot.Amounts.Min();
            var sidePot = new SidePot();
            foreach (var nickname in totalPot.Nicknames)
            {
                var deadAmount = totalPot.GetDead();
                if (deadAmount)
                {
                    totalPot = totalPot.SubDead(deadAmount);
                    sidePot = sidePot.AddDead(deadAmount);
                }

                totalPot = totalPot.Sub(nickname, minAmount);
                if (nicknames.Contains(nickname))
                {
                    sidePot = sidePot.Add(nickname, minAmount);
                }
                else
                {
                    sidePot = sidePot.AddDead(minAmount);
                }
            }

            if (sidePots.Count > 0 && sidePots[^1].Nicknames.ToHashSet() == sidePot.Nicknames.ToHashSet())
            {
                // If the previous side pot contains the same nicknames only, we merge it with the current one
                sidePots[^1] = sidePots[^1].Merge(sidePot);
            }
            else
            {
                // Otherwise, it will be a new side pot
                sidePots.Add(sidePot);                
            }
        }

        return sidePots;
    }
    
    public bool FoldIsAvailable(Player player)
    {
        try
        {
            ValidateFold(player);
            return true;
        }
        catch (NotAvailableError)
        {
            return false;
        }
    }

    public bool CheckIsAvailable(Player player)
    {
        try
        {
            ValidateCheck(player);
            return true;
        }
        catch (NotAvailableError)
        {
            return false;
        }
    }

    public bool CallIsAvailable(Player player)
    {
        var amount = GetCallToAmount(player);
        try
        {
            ValidateCallTo(player, amount);
            return true;
        }
        catch (NotAvailableError)
        {
            return false;
        }
    }

    public bool RaiseIsAvailable(Player player)
    {
        var amount = GetMinRaiseToAmount(player);
        try
        {
            ValidateRaiseTo(player, amount);
            return true;
        }
        catch (NotAvailableError)
        {
            return false;
        }
    }

    public bool DecisionIsAvailable(Player player)
    {
        return (
            FoldIsAvailable(player)
            || CheckIsAvailable(player)
            || CallIsAvailable(player)
            || RaiseIsAvailable(player)
        );
    }

    protected Chips GetCurrentMaxAmount(Player player)
    {
        var maxAmount = new Chips(0);

        foreach (var (nickname, amount) in _currentSidePot)
        {
            if (nickname == player.Nickname)
            {
                continue;
            }

            if (amount > maxAmount)
            {
                maxAmount = amount;
            }
        }

        return maxAmount;
    }

    private Chips GetPreviousMaxAmount(Player player)
    {
        var maxAmount = new Chips(0);

        foreach (var (nickname, amount) in _previousSidePot)
        {
            if (nickname == player.Nickname)
            {
                continue;
            }

            if (amount > maxAmount)
            {
                maxAmount = amount;
            }
        }

        return maxAmount;
    }

    private void ValidatePostBlind(Player player, Chips amount, Chips blind)
    {
        var expectedAmount = (blind <= player.Stake) ? blind : player.Stake;
        if (amount != expectedAmount)
        {
            throw new NotAvailableError($"The player must post {expectedAmount}");
        }
    }

    private void ValidateFold(Player player)
    {
        var currentPostedAmount = GetCurrentPostedAmount(player);
        var currentMaxAmount = GetCurrentMaxAmount(player);

        if (currentPostedAmount > currentMaxAmount)
        {
            throw new NotAvailableError("The player has posted the most amount into the pot, he cannot fold");
        }

        if (currentPostedAmount == currentMaxAmount)
        {
            throw new NotAvailableError("The player has posted the same amount into the pot, he cannot fold");
        }
    }

    private void ValidateCheck(Player player)
    {
        var currentPostedAmount = GetCurrentPostedAmount(player);
        var currentMaxAmount = GetCurrentMaxAmount(player);

        if (currentPostedAmount > currentMaxAmount)
        {
            throw new NotAvailableError("The player has posted the most amount into the pot, he cannot check");
        }

        if (currentPostedAmount < currentMaxAmount)
        {
            throw new NotAvailableError("The player has posted less amount into the pot, he cannot check");
        }

        // Covers a case when the player is on a blind and there was a limp 
        if (IsCurrentDecisionCommitted(player))
        {
            throw new NotAvailableError("The player has already performed an action, he cannot check");
        }
    }

    private void ValidateCallTo(Player player, Chips amount)
    {
        var currentPostedAmount = GetCurrentPostedAmount(player);
        var currentMaxAmount = GetCurrentMaxAmount(player);

        if (currentPostedAmount > currentMaxAmount)
        {
            throw new NotAvailableError("The player has posted the most amount into the pot, he cannot call");
        }

        if (currentPostedAmount == currentMaxAmount)
        {
            throw new NotAvailableError("The player has posted the same amount into the pot, he cannot call");
        }

        var expectedAmount = GetCallToAmount(player);
        if (amount != expectedAmount)
        {
            throw new NotAvailableError($"The player must call to {expectedAmount}");
        }
    }

    private void ValidateRaiseTo(Player player, Chips amount)
    {
        var currentPostedAmount = GetCurrentPostedAmount(player);
        var currentMaxAmount = GetCurrentMaxAmount(player);

        if (currentPostedAmount > currentMaxAmount)
        {
            throw new NotAvailableError("The player has posted the most amount into the pot, he cannot raise");
        }

        if (IsCurrentDecisionCommitted(player) && !ThereWasRaiseSincePlayerDecision(player))
        {
            throw new NotAvailableError("There was no raise since the player's last action, he cannot raise");
        }

        var minExpectedAmount = GetMinRaiseToAmount(player);
        if (minExpectedAmount < currentMaxAmount)
        {
            throw new NotAvailableError($"The player must call to {minExpectedAmount}");
        }
        if (amount < minExpectedAmount)
        {
            throw new NotAvailableError($"The player must raise to minimum {minExpectedAmount}");
        }

        var maxExpectedAmount = GetMaxRaiseToAmount(player);
        if (amount > maxExpectedAmount)
        {
            throw new NotAvailableError($"The player must raise to maximum {maxExpectedAmount}");
        }
    }

    private void ValidateRefund(Player player, Chips amount)
    {
        var expectedAmount = GetRefundAmount(player);

        if (!expectedAmount)
        {
            throw new NotAvailableError("The player cannot refund");
        }

        if (amount != expectedAmount)
        {
            throw new NotAvailableError($"The player must refund {expectedAmount}");
        }
    }

    private void ValidateWinWithoutShowdown(Player player, Chips amount)
    {
        var expectedAmount = GetTotalAmount();
        if (amount != expectedAmount)
        {
            throw new NotAvailableError($"The player must win {expectedAmount}");
        }
    }

    private void ValidateWinAtShowdown(IList<(Player, Combo, Chips)> playerComboAmounts)
    {
        if (playerComboAmounts.Count <= 1)
        {
            throw new NotAvailableError("There must be at least two players to win at showdown");
        }

        var playerCombos = playerComboAmounts.Select(x => (x.Item1, x.Item2)).ToList();
        var expectedPlayerComboAmounts = GetWinAtShowdownAmounts(playerCombos);

        var playerComboAmountSet = playerComboAmounts.ToHashSet();
        var expectedPlayerComboAmountSet = expectedPlayerComboAmounts.ToHashSet();

        if (playerComboAmountSet == expectedPlayerComboAmountSet)
        {
            return;
        }

        var remainingPlayerComboAmounts = playerComboAmountSet.Except(expectedPlayerComboAmountSet);
        var expectedRemainingPlayerComboAmounts = expectedPlayerComboAmountSet.Except(playerComboAmountSet);

        foreach (var (player, combo, amount) in expectedRemainingPlayerComboAmounts)
        {
            throw new NotAvailableError($"Player {player.Nickname} with combo {combo} must win {amount}");
        }

        foreach (var (player, combo, _) in remainingPlayerComboAmounts)
        {
            throw new NotAvailableError($"Player {player.Nickname} with combo {combo} must not win");
        }
    }

    private void PostTo(Player player, Chips amount)
    {
        var remainingAmount = amount - GetCurrentPostedAmount(player);
        player.Post(remainingAmount);

        _currentSidePot = _currentSidePot.Add(player.Nickname, remainingAmount);
    }

    private void CommitCurrentDecision(Player player)
    {
        _currentDecisionCommittedNicknames.Add(player.Nickname);
    }

    private bool IsCurrentDecisionCommitted(Player player)
    {
        return _currentDecisionCommittedNicknames.Contains(player.Nickname);
    }

    private bool ThereWasRaiseSincePlayerDecision(Player player)
    {
        return (
            IsCurrentDecisionCommitted(player)
            && GetCurrentPostedAmount(player) < GetCurrentMaxAmount(player)
            && player.Nickname != _lastRaiseNickname
        );
    }
}

public class NoLimitPot : BasePot
{
    public NoLimitPot(
        Chips smallBlind,
        Chips bigBlind,
        Nickname? lastDecisionNickname,
        Nickname? lastRaiseNickname,
        Chips lastRaiseStep,
        IEnumerable<Nickname> currentDecisionNicknames,
        SidePot currentSidePot,
        SidePot previousSidePot
    ) : base(smallBlind, bigBlind, lastDecisionNickname, lastRaiseNickname, lastRaiseStep, currentDecisionNicknames, currentSidePot, previousSidePot)
    {
    }

    public override Chips GetMaxRaiseToAmount(Player player)
    {
        return GetCurrentPostedAmount(player) + player.Stake;
    }

    public override string ToString()
        => $"{GetType().Name}, {GetTotalAmount()}";
}

public class PotLimitPot : BasePot
{
    public PotLimitPot(
        Chips smallBlind,
        Chips bigBlind,
        Nickname? lastDecisionNickname,
        Nickname? lastRaiseNickname,
        Chips lastRaiseStep,
        IEnumerable<Nickname> currentDecisionNicknames,
        SidePot currentSidePot,
        SidePot previousSidePot
    ) : base(smallBlind, bigBlind, lastDecisionNickname, lastRaiseNickname, lastRaiseStep, currentDecisionNicknames, currentSidePot, previousSidePot)
    {
    }

    public override Chips GetMaxRaiseToAmount(Player player)
    {
        var currentMaxAmount = GetCurrentMaxAmount(player);
        var currentPostedAmount = GetCurrentPostedAmount(player);
        var totalAmountAfterCall = GetTotalAmount() + currentMaxAmount - currentPostedAmount;
        var maxAmount = currentMaxAmount + totalAmountAfterCall;
        var amount = currentMaxAmount + maxAmount;
        var playerAmount = currentPostedAmount + player.Stake;
        return amount < playerAmount ? amount : playerAmount;
    }
}