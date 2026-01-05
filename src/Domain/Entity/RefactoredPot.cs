using Domain.ValueObject;

namespace Domain.Entity;

public class Pot
{
    public Chips MinBet { get; }

    private Chips Ante { get; set; } = new(0);
    private Bets CommittedBets { get; } = new();
    private Bets UncommittedBets { get; } = new();

    public Nickname? LastPostedNickname { get; private set; }
    public Nickname? LastRaisedNickname { get; private set; }
    public Chips LastRaisedStep { get; private set; }
    private HashSet<Nickname> PostedUncommittedBetNicknames { get; } = new();

    public Chips TotalAmount => Ante + CommittedBets.TotalAmount + UncommittedBets.TotalAmount;

    public Pot(Chips minBet)
    {
        MinBet = minBet;
        LastRaisedStep = minBet;
    }

    public void PostAnte(Chips amount)
    {
        Ante += amount;
    }

    public void PostBlind(Nickname nickname, Chips amount)
    {
        UncommittedBets.Post(nickname, amount);

        LastPostedNickname = nickname;
        LastRaisedNickname = nickname;

        if (amount > LastRaisedStep)
        {
            // Covers the case of posting small blind or auto-all-in on the blind
            LastRaisedStep = amount;
        }
    }

    public void PostBet(Nickname nickname, Chips amount)
    {
        var playerPosted = amount + UncommittedBets.GetAmountPostedBy(nickname);
        var maxPosted = UncommittedBets.GetMaxAmountPostedNotBy(nickname);

        UncommittedBets.Post(nickname, amount);
        LastPostedNickname = nickname;
        PostedUncommittedBetNicknames.Add(nickname);

        if (playerPosted >= maxPosted + LastRaisedStep)
        {
            LastRaisedNickname = nickname;
            LastRaisedStep = playerPosted - maxPosted;
        }
    }

    public void RefundBet(Nickname nickname, Chips amount)
    {
        UncommittedBets.Refund(nickname, amount);
    }

    public void CommitBets()
    {
        LastPostedNickname = null;
        LastRaisedNickname = null;
        LastRaisedStep = MinBet;
        PostedUncommittedBetNicknames.Clear();

        CommittedBets.MergeWith(UncommittedBets);
        UncommittedBets.Clear();
    }

    public Chips GetUncommittedAmountPostedBy(Nickname nickname)
    {
        return UncommittedBets.GetAmountPostedBy(nickname);
    }

    public Chips GetUncommittedMaxAmountPostedNotBy(Nickname nickname)
    {
        return UncommittedBets.GetMaxAmountPostedNotBy(nickname);
    }

    public bool PostedUncommittedBet(Nickname nickname)
    {
        return PostedUncommittedBetNicknames.Contains(nickname);
    }

    public (Nickname?, Chips) CalculateRefund()
    {
        var maxNickname = UncommittedBets.GetNicknamePostedMaxAmount();
        if (maxNickname is null)
        {
            return (null, new Chips(0));
        }

        var nickname = (Nickname)maxNickname;
        var maxAmount = UncommittedBets.GetAmountPostedBy(nickname);
        var secondMaxAmount = UncommittedBets.GetMaxAmountPostedNotBy(nickname);

        if (maxAmount > secondMaxAmount)
        {
            return (nickname, maxAmount - secondMaxAmount);
        }

        return (null, new Chips(0));
    }

    public override string ToString() =>
        $"{GetType().Name}: {TotalAmount}";
}

internal class Bets
{
    private readonly Dictionary<Nickname, Chips> _mapping = new();
    public Chips TotalAmount => _mapping.Values.Sum(x => x);

    public Chips GetAmountPostedBy(Nickname nickname)
    {
        if (_mapping.TryGetValue(nickname, out var amount))
        {
            return amount;
        }

        return new Chips(0);
    }

    public Chips GetMaxAmountPostedNotBy(Nickname nickname)
    {
        var posted = _mapping.Where(kv => kv.Key != nickname).Select(kv => kv.Value).ToList();
        return posted.Count > 0 ? posted.Max(x => x) : new Chips(0);
    }

    public Nickname? GetNicknamePostedMaxAmount()
    {
        if (_mapping.Count == 0)
        {
            return null;
        }

        var maxAmount = _mapping.Values.Max();
        return _mapping.First(kv => kv.Value.Equals(maxAmount)).Key;
    }

    public void Post(Nickname nickname, Chips amount)
    {
        _mapping[nickname] = GetAmountPostedBy(nickname) + amount;
    }

    public void Refund(Nickname nickname, Chips amount)
    {
        if (amount > GetAmountPostedBy(nickname))
        {
            throw new InvalidOperationException("Cannot refund more than posted");
        }

        _mapping[nickname] = GetAmountPostedBy(nickname) - amount;
    }

    public void MergeWith(Bets other)
    {
        foreach (var (nickname, amount) in other._mapping)
        {
            Post(nickname, amount);
        }
    }

    public void Clear()
    {
        _mapping.Clear();
    }
}
