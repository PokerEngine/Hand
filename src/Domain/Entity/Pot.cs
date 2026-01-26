using Domain.ValueObject;

namespace Domain.Entity;

public class Pot
{
    public Chips MinBet { get; }
    private Chips Ante { get; set; } = Chips.Zero;
    private Bets CollectedBets { get; set; } = new();
    private Bets CurrentBets { get; set; } = new();
    private List<Award> Awards { get; } = new();

    public Nickname? LastPostedNickname { get; private set; }
    public Nickname? LastRaisedNickname { get; private set; }
    public Chips LastRaisedStep { get; private set; }
    private HashSet<Nickname> PostedCurrentBetNicknames { get; } = new();

    public Chips TotalAmount => Ante + CollectedBets.TotalAmount + CurrentBets.TotalAmount;

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
        CurrentBets = CurrentBets.Post(nickname, amount);

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
        var playerPosted = amount + CurrentBets.GetAmountPostedBy(nickname);
        var maxPosted = CurrentBets.GetMaxAmountPostedNotBy(nickname);

        CurrentBets = CurrentBets.Post(nickname, amount);
        LastPostedNickname = nickname;
        PostedCurrentBetNicknames.Add(nickname);

        if (playerPosted >= maxPosted + LastRaisedStep)
        {
            LastRaisedNickname = nickname;
            LastRaisedStep = playerPosted - maxPosted;
        }
    }

    public void RefundBet(Nickname nickname, Chips amount)
    {
        CurrentBets = CurrentBets.Refund(nickname, amount);
    }

    public bool HasCurrentBets()
    {
        return !CurrentBets.TotalAmount.IsZero;
    }

    public void CollectBets()
    {
        LastPostedNickname = null;
        LastRaisedNickname = null;
        LastRaisedStep = MinBet;
        PostedCurrentBetNicknames.Clear();

        CollectedBets += CurrentBets;
        CurrentBets = new Bets();
    }

    public void WinSidePot(SidePot sidePot, HashSet<Nickname> winners)
    {
        var amount = sidePot.TotalAmount;
        Ante -= sidePot.Ante;
        CollectedBets -= sidePot.Bets;

        var award = new Award
        {
            Winners = winners,
            Amount = amount
        };
        Awards.Add(award);
    }

    public Chips GetCurrentAmountPostedBy(Nickname nickname)
    {
        return CurrentBets.GetAmountPostedBy(nickname);
    }

    public Chips GetCurrentMaxAmountPostedNotBy(Nickname nickname)
    {
        return CurrentBets.GetMaxAmountPostedNotBy(nickname);
    }

    public bool PostedCurrentBet(Nickname nickname)
    {
        return PostedCurrentBetNicknames.Contains(nickname);
    }

    public (Nickname?, Chips) CalculateRefund()
    {
        var maxNickname = CurrentBets.GetNicknamePostedMaxAmount();
        if (maxNickname is null)
        {
            return (null, Chips.Zero);
        }

        var nickname = (Nickname)maxNickname;
        var maxAmount = CurrentBets.GetAmountPostedBy(nickname);
        var secondMaxAmount = CurrentBets.GetMaxAmountPostedNotBy(nickname);

        if (maxAmount > secondMaxAmount)
        {
            return (nickname, maxAmount - secondMaxAmount);
        }

        return (null, Chips.Zero);
    }

    public IEnumerable<SidePot> CalculateSidePots(HashSet<Nickname> competitors)
    {
        var ante = Ante;
        var totalBets = CollectedBets + CurrentBets;

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

            ante = Chips.Zero;
        }
    }

    public PotState GetState()
    {
        return new PotState
        {
            Ante = Ante,
            CollectedBets = CollectedBets.Select(x => new BetState { Nickname = x.Key, Amount = x.Value }).ToList(),
            CurrentBets = CurrentBets.Select(x => new BetState { Nickname = x.Key, Amount = x.Value }).ToList(),
            Awards = Awards.Select(x => new AwardState { Nicknames = x.Winners.ToList(), Amount = x.Amount }).ToList()
        };
    }

    public override string ToString() =>
        $"{GetType().Name}: {TotalAmount}";
}
