using Domain.ValueObject;

namespace Domain.Entity;

public class Pot
{
    public Chips MinBet { get; }
    private Chips Ante { get; set; } = Chips.Zero;
    private Bets CommittedBets { get; set; } = new();
    private Bets UncommittedBets { get; set; } = new();
    private List<Award> Awards { get; } = new();

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
        UncommittedBets = UncommittedBets.Post(nickname, amount);

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

        UncommittedBets = UncommittedBets.Post(nickname, amount);
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
        UncommittedBets = UncommittedBets.Refund(nickname, amount);
    }

    public void CommitBets()
    {
        LastPostedNickname = null;
        LastRaisedNickname = null;
        LastRaisedStep = MinBet;
        PostedUncommittedBetNicknames.Clear();

        CommittedBets += UncommittedBets;
        UncommittedBets = new Bets();
    }

    public void WinSidePot(SidePot sidePot, HashSet<Nickname> winners)
    {
        var amount = sidePot.TotalAmount;
        Ante -= sidePot.Ante;
        CommittedBets -= sidePot.Bets;

        var award = new Award
        {
            Winners = winners,
            Amount = amount
        };
        Awards.Add(award);
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

    public IEnumerable<SidePot> CalculateSidePots(HashSet<Nickname> competitors)
    {
        var ante = Ante;
        var totalBets = CommittedBets + UncommittedBets;

        if (totalBets.TotalAmount.IsZero && !ante.IsZero)
        {
            // Only ante is in the pot, should distribute between all competitors
            yield return new SidePot(competitors, totalBets, ante);
            yield break;
        }

        while (!totalBets.TotalAmount.IsZero)
        {
            var sidePotBets = new Bets();
            var sidePotCompetitors = new HashSet<Nickname>();

            var edgeAmount = competitors.Select(n => totalBets.GetAmountPostedBy(n)).Where(a => !a.IsZero).Min();

            foreach (var (n, a) in totalBets)
            {
                if (a.IsZero)
                {
                    continue;
                }

                var amount = a < edgeAmount ? a : edgeAmount;
                totalBets = totalBets.Refund(n, amount);
                sidePotBets = sidePotBets.Post(n, amount);

                if (competitors.Contains(n))
                {
                    sidePotCompetitors.Add(n);
                }
            }

            yield return new SidePot(sidePotCompetitors, sidePotBets, ante);

            ante = new Chips(0);
        }
    }

    public PotState GetState()
    {
        return new PotState
        {
            Ante = Ante,
            CommittedBets = CommittedBets.Select(x => new BetState { Nickname = x.Key, Amount = x.Value }).ToList(),
            UncommittedBets = UncommittedBets.Select(x => new BetState { Nickname = x.Key, Amount = x.Value }).ToList(),
            Awards = Awards.Select(x => new AwardState { Nicknames = x.Winners.ToList(), Amount = x.Amount }).ToList()
        };
    }

    public override string ToString() =>
        $"{GetType().Name}: {TotalAmount}";
}
