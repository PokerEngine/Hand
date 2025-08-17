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

    protected BasePot(Chips smallBlind, Chips bigBlind)
    {
        SmallBlind = smallBlind;
        BigBlind = bigBlind;
        LastDecisionNickname = null;

        _lastRaiseNickname = null;
        _lastRaiseStep = bigBlind;
        _currentDecisionCommittedNicknames = new HashSet<Nickname>();
        _currentSidePot = new SidePot();
        _previousSidePot = new SidePot();
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

    public void CommitDecision(Player player, Decision decision)
    {
        switch (decision.Type)
        {
            case DecisionType.Fold:
                Fold(player);
                break;
            case DecisionType.Check:
                Check(player);
                break;
            case DecisionType.CallTo:
                CallTo(player, decision.Amount);
                break;
            case DecisionType.RaiseTo:
                RaiseTo(player, decision.Amount);
                break;
            default:
                throw new ArgumentException("The decision is unknown", nameof(decision));
        }
    }

    public void CommitRefund(Player player, Chips amount)
    {
        ValidateRefund(player, amount);

        _previousSidePot = _previousSidePot.Sub(player.Nickname, amount);
        player.Refund(amount);
    }

    public void CommitWinWithoutShowdown(Player player, Chips amount)
    {
        ValidateWinWithoutShowdown(player, amount);

        _previousSidePot = new SidePot();
        player.Win(amount);
    }

    public void CommitWinAtShowdown(IList<Player> players, SidePot sidePot, SidePot winPot)
    {
        ValidateWinAtShowdown(players, sidePot, winPot);

        foreach (var (nickname, amount) in sidePot)
        {
            _previousSidePot = _previousSidePot.Sub(nickname, amount);
        }

        foreach (var player in players)
        {
            var amount = winPot.Get(player.Nickname);
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

    public SidePot GetWinPot(IList<Player> players, SidePot sidePot)
    {
        // More than 1 players means that they should split the pot
        var quotient = sidePot.Amount / players.Count;
        var remainder = sidePot.Amount % players.Count;

        var winPot = new SidePot();
        var oneChip = new Chips(1);

        foreach (var player in players.OrderBy(x => x.Stake))
        {
            winPot = winPot.Add(player.Nickname, quotient);

            if (remainder)
            {
                // We distribute the remainder among the players starting from the poorest one
                winPot = winPot.Add(player.Nickname, oneChip);
                remainder -= oneChip;
            }
        }

        return winPot;
    }

    public IList<SidePot> GetSidePots(IList<Player> players)
    {
        return GetSidePots(players.Select(x => x.Nickname).ToList());
    }

    private IList<SidePot> GetSidePots(IEnumerable<Nickname> nicknames)
    {
        var remainingPot = _previousSidePot.Merge(_currentSidePot);
        var sortedNicknames = nicknames.OrderBy(x => remainingPot.Get(x)).ThenBy(x => x).ToList();

        var oneChip = new Chips(1);
        var sidePots = new List<SidePot>();

        while (sortedNicknames.Count > 0)
        {
            var poorestNickname = sortedNicknames.First();
            var poorestAmount = remainingPot.Get(poorestNickname);
            if (!poorestAmount)
            {
                sortedNicknames.RemoveAt(0);
                continue;
            }

            var sidePotAmount = new Chips(0);

            foreach (var (nickname, amount) in remainingPot)
            {
                var minAmount = poorestAmount <= amount ? poorestAmount : amount;
                remainingPot = remainingPot.Sub(nickname, minAmount);
                sidePotAmount += minAmount;
            }

            var quotient = sidePotAmount / sortedNicknames.Count;
            var remainder = sidePotAmount % sortedNicknames.Count;

            var sidePot = new SidePot();

            foreach (var nickname in sortedNicknames)
            {
                sidePot = sidePot.Add(nickname, quotient);

                if (remainder)
                {
                    // We distribute the remainder among the players starting from the poorest one
                    sidePot = sidePot.Add(nickname, oneChip);
                    remainder -= oneChip;
                }
            }

            sidePots.Add(sidePot);
            sortedNicknames.RemoveAt(0);
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
        catch (InvalidOperationException)
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
        catch (InvalidOperationException)
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
        catch (InvalidOperationException)
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
        catch (InvalidOperationException)
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

    private void Fold(Player player)
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

    private void Check(Player player)
    {
        ValidateCheck(player);

        player.Check();
        CommitCurrentDecision(player);
        LastDecisionNickname = player.Nickname;
    }

    private void CallTo(Player player, Chips amount)
    {
        ValidateCallTo(player, amount);

        BetTo(player, amount);
        CommitCurrentDecision(player);
        LastDecisionNickname = player.Nickname;
    }

    private void RaiseTo(Player player, Chips amount)
    {
        ValidateRaiseTo(player, amount);

        var raiseStep = new Chips(0);
        var currentMaxAmount = GetCurrentMaxAmount(player);
        if (amount >= currentMaxAmount + _lastRaiseStep)
        {
            raiseStep = amount - currentMaxAmount;
        }

        BetTo(player, amount);
        CommitCurrentDecision(player);
        LastDecisionNickname = player.Nickname;

        // If a player raises less than the minimum amount, he goes all in, and it is not considered a raise
        if (raiseStep)
        {
            _lastRaiseStep = raiseStep;
            _lastRaiseNickname = player.Nickname;
        }
    }

    private void ValidatePostBlind(Player player, Chips amount, Chips blind)
    {
        var expectedAmount = (blind <= player.Stake) ? blind : player.Stake;
        if (amount != expectedAmount)
        {
            throw new InvalidOperationException($"The player must post {expectedAmount}");
        }
    }

    private void ValidateFold(Player player)
    {
        var currentPostedAmount = GetCurrentPostedAmount(player);
        var currentMaxAmount = GetCurrentMaxAmount(player);

        if (currentPostedAmount > currentMaxAmount)
        {
            throw new InvalidOperationException("The player has posted the most amount into the pot, he cannot fold");
        }

        if (currentPostedAmount == currentMaxAmount)
        {
            throw new InvalidOperationException("The player has posted the same amount into the pot, he cannot fold");
        }
    }

    private void ValidateCheck(Player player)
    {
        var currentPostedAmount = GetCurrentPostedAmount(player);
        var currentMaxAmount = GetCurrentMaxAmount(player);

        if (currentPostedAmount > currentMaxAmount)
        {
            throw new InvalidOperationException("The player has posted the most amount into the pot, he cannot check");
        }

        if (currentPostedAmount < currentMaxAmount)
        {
            throw new InvalidOperationException("The player has posted less amount into the pot, he cannot check");
        }

        // Covers a case when the player is on a blind and there was a limp 
        if (IsCurrentDecisionCommitted(player))
        {
            throw new InvalidOperationException("The player has already performed an action, he cannot check");
        }
    }

    private void ValidateCallTo(Player player, Chips amount)
    {
        var currentPostedAmount = GetCurrentPostedAmount(player);
        var currentMaxAmount = GetCurrentMaxAmount(player);

        if (currentPostedAmount > currentMaxAmount)
        {
            throw new InvalidOperationException("The player has posted the most amount into the pot, he cannot call");
        }

        if (currentPostedAmount == currentMaxAmount)
        {
            throw new InvalidOperationException("The player has posted the same amount into the pot, he cannot call");
        }

        var expectedAmount = GetCallToAmount(player);
        if (amount != expectedAmount)
        {
            throw new InvalidOperationException($"The player must call to {expectedAmount}");
        }
    }

    private void ValidateRaiseTo(Player player, Chips amount)
    {
        var currentPostedAmount = GetCurrentPostedAmount(player);
        var currentMaxAmount = GetCurrentMaxAmount(player);

        if (currentPostedAmount > currentMaxAmount)
        {
            throw new InvalidOperationException("The player has posted the most amount into the pot, he cannot raise");
        }

        if (IsCurrentDecisionCommitted(player) && !ThereWasRaiseSincePlayerDecision(player))
        {
            throw new InvalidOperationException("There was no raise since the player's last action, he cannot raise");
        }

        var minExpectedAmount = GetMinRaiseToAmount(player);
        if (minExpectedAmount < currentMaxAmount)
        {
            throw new InvalidOperationException($"The player must call to {minExpectedAmount}");
        }
        if (amount < minExpectedAmount)
        {
            throw new InvalidOperationException($"The player must raise to minimum {minExpectedAmount}");
        }

        var maxExpectedAmount = GetMaxRaiseToAmount(player);
        if (amount > maxExpectedAmount)
        {
            throw new InvalidOperationException($"The player must raise to maximum {maxExpectedAmount}");
        }
    }

    private void ValidateRefund(Player player, Chips amount)
    {
        var expectedAmount = GetRefundAmount(player);

        if (!expectedAmount)
        {
            throw new InvalidOperationException("The player cannot refund");
        }

        if (amount != expectedAmount)
        {
            throw new InvalidOperationException($"The player must refund {expectedAmount}");
        }
    }

    private void ValidateWinWithoutShowdown(Player player, Chips amount)
    {
        var expectedAmount = GetTotalAmount();
        if (amount != expectedAmount)
        {
            throw new InvalidOperationException($"The player must win {expectedAmount}");
        }
    }

    private void ValidateWinAtShowdown(IList<Player> players, SidePot sidePot, SidePot winPot)
    {
        var expectedWinPot = GetWinPot(players, sidePot);
        if (expectedWinPot.Amount != winPot.Amount)
        {
            throw new InvalidOperationException($"The player(s) must win {expectedWinPot.Amount}");
        }
        if (!expectedWinPot.Equals(winPot))
        {
            throw new InvalidOperationException($"The player(s) must win {expectedWinPot}");
        }
    }

    private void PostTo(Player player, Chips amount)
    {
        var remainingAmount = amount - GetCurrentPostedAmount(player);
        player.Post(remainingAmount);

        _currentSidePot = _currentSidePot.Add(player.Nickname, remainingAmount);
    }

    private void BetTo(Player player, Chips amount)
    {
        var remainingAmount = amount - GetCurrentPostedAmount(player);
        player.Bet(remainingAmount);

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
    public NoLimitPot(Chips smallBlind, Chips bigBlind) : base(smallBlind, bigBlind)
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
    public PotLimitPot(Chips smallBlind, Chips bigBlind) : base(smallBlind, bigBlind)
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
