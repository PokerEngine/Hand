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
        var playerPosted = amount + UncommittedBets.PostedBy(nickname);
        var maxPosted = UncommittedBets.MaxPosted;

        UncommittedBets.Post(nickname, amount);
        LastPostedNickname = nickname;
        PostedUncommittedBetNicknames.Add(nickname);

        if (playerPosted >= maxPosted + LastRaisedStep)
        {
            LastRaisedNickname = nickname;
            LastRaisedStep = playerPosted - maxPosted;
        }
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

    public bool PostedUncommittedBet(Nickname nickname)
    {
        return PostedUncommittedBetNicknames.Contains(nickname);
    }
}

internal class Bets
{
    private readonly Dictionary<Nickname, Chips> _mapping = new();
    public Chips TotalAmount => _mapping.Values.Sum(x => x);
    public Chips MaxPosted => _mapping.Count > 0 ? _mapping.Values.Max(x => x) : new Chips(0);

    public Chips PostedBy(Nickname nickname)
    {
        if (_mapping.TryGetValue(nickname, out var amount))
        {
            return amount;
        }

        return new Chips(0);
    }

    public void Post(Nickname nickname, Chips amount)
    {
        _mapping[nickname] = PostedBy(nickname) + amount;
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
