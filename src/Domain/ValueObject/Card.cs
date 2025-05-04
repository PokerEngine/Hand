namespace Domain.ValueObject;

public enum Rank
{
    Ace = 14,
    King = 13,
    Queen = 12,
    Jack = 11,
    Ten = 10,
    Nine = 9,
    Eight = 8,
    Seven = 7,
    Six = 6,
    Five = 5,
    Four = 4,
    Trey = 3,
    Deuce = 2
}

public enum Suit
{
    Spades = 4,
    Hearts = 3,
    Diamonds = 2,
    Clubs = 1
}

public readonly struct Card : IEquatable<Card>, IComparable<Card>
{
    public readonly Rank Rank;
    public readonly Suit Suit;

    public static readonly Card AceOfSpades = new(Rank.Ace, Suit.Spades);
    public static readonly Card KingOfSpades = new(Rank.King, Suit.Spades);
    public static readonly Card QueenOfSpades = new(Rank.Queen, Suit.Spades);
    public static readonly Card JackOfSpades = new(Rank.Jack, Suit.Spades);
    public static readonly Card TenOfSpades = new(Rank.Ten, Suit.Spades);
    public static readonly Card NineOfSpades = new(Rank.Nine, Suit.Spades);
    public static readonly Card EightOfSpades = new(Rank.Eight, Suit.Spades);
    public static readonly Card SevenOfSpades = new(Rank.Seven, Suit.Spades);
    public static readonly Card SixOfSpades = new(Rank.Six, Suit.Spades);
    public static readonly Card FiveOfSpades = new(Rank.Five, Suit.Spades);
    public static readonly Card FourOfSpades = new(Rank.Four, Suit.Spades);
    public static readonly Card TreyOfSpades = new(Rank.Trey, Suit.Spades);
    public static readonly Card DeuceOfSpades = new(Rank.Deuce, Suit.Spades);
    public static readonly Card AceOfHearts = new(Rank.Ace, Suit.Hearts);
    public static readonly Card KingOfHearts = new(Rank.King, Suit.Hearts);
    public static readonly Card QueenOfHearts = new(Rank.Queen, Suit.Hearts);
    public static readonly Card JackOfHearts = new(Rank.Jack, Suit.Hearts);
    public static readonly Card TenOfHearts = new(Rank.Ten, Suit.Hearts);
    public static readonly Card NineOfHearts = new(Rank.Nine, Suit.Hearts);
    public static readonly Card EightOfHearts = new(Rank.Eight, Suit.Hearts);
    public static readonly Card SevenOfHearts = new(Rank.Seven, Suit.Hearts);
    public static readonly Card SixOfHearts = new(Rank.Six, Suit.Hearts);
    public static readonly Card FiveOfHearts = new(Rank.Five, Suit.Hearts);
    public static readonly Card FourOfHearts = new(Rank.Four, Suit.Hearts);
    public static readonly Card TreyOfHearts = new(Rank.Trey, Suit.Hearts);
    public static readonly Card DeuceOfHearts = new(Rank.Deuce, Suit.Hearts);
    public static readonly Card AceOfDiamonds = new(Rank.Ace, Suit.Diamonds);
    public static readonly Card KingOfDiamonds = new(Rank.King, Suit.Diamonds);
    public static readonly Card QueenOfDiamonds = new(Rank.Queen, Suit.Diamonds);
    public static readonly Card JackOfDiamonds = new(Rank.Jack, Suit.Diamonds);
    public static readonly Card TenOfDiamonds = new(Rank.Ten, Suit.Diamonds);
    public static readonly Card NineOfDiamonds = new(Rank.Nine, Suit.Diamonds);
    public static readonly Card EightOfDiamonds = new(Rank.Eight, Suit.Diamonds);
    public static readonly Card SevenOfDiamonds = new(Rank.Seven, Suit.Diamonds);
    public static readonly Card SixOfDiamonds = new(Rank.Six, Suit.Diamonds);
    public static readonly Card FiveOfDiamonds = new(Rank.Five, Suit.Diamonds);
    public static readonly Card FourOfDiamonds = new(Rank.Four, Suit.Diamonds);
    public static readonly Card TreyOfDiamonds = new(Rank.Trey, Suit.Diamonds);
    public static readonly Card DeuceOfDiamonds = new(Rank.Deuce, Suit.Diamonds);
    public static readonly Card AceOfClubs = new(Rank.Ace, Suit.Clubs);
    public static readonly Card KingOfClubs = new(Rank.King, Suit.Clubs);
    public static readonly Card QueenOfClubs = new(Rank.Queen, Suit.Clubs);
    public static readonly Card JackOfClubs = new(Rank.Jack, Suit.Clubs);
    public static readonly Card TenOfClubs = new(Rank.Ten, Suit.Clubs);
    public static readonly Card NineOfClubs = new(Rank.Nine, Suit.Clubs);
    public static readonly Card EightOfClubs = new(Rank.Eight, Suit.Clubs);
    public static readonly Card SevenOfClubs = new(Rank.Seven, Suit.Clubs);
    public static readonly Card SixOfClubs = new(Rank.Six, Suit.Clubs);
    public static readonly Card FiveOfClubs = new(Rank.Five, Suit.Clubs);
    public static readonly Card FourOfClubs = new(Rank.Four, Suit.Clubs);
    public static readonly Card TreyOfClubs = new(Rank.Trey, Suit.Clubs);
    public static readonly Card DeuceOfClubs = new(Rank.Deuce, Suit.Clubs);

    private static readonly Dictionary<Rank, char> RankMapping = new()
    {
        { Rank.Ace, 'A' },
        { Rank.King, 'K' },
        { Rank.Queen, 'Q' },
        { Rank.Jack, 'J' },
        { Rank.Ten, 'T' },
        { Rank.Nine, '9' },
        { Rank.Eight, '8' },
        { Rank.Seven, '7' },
        { Rank.Six, '6' },
        { Rank.Five, '5' },
        { Rank.Four, '4' },
        { Rank.Trey, '3' },
        { Rank.Deuce, '2' }
    };
    private static readonly Dictionary<Suit, char> SuitMapping = new()
    {
        { Suit.Spades, 's' },
        { Suit.Hearts, 'h' },
        { Suit.Diamonds, 'd' },
        { Suit.Clubs, 'c' },
    };

    private Card(Rank rank, Suit suit)
    {
        Rank = rank;
        Suit = suit;
    }

    public int CompareTo(Card other)
    {
        var result = -Rank.CompareTo(other.Rank);
        if (result == 0)
        {
            result = -Suit.CompareTo(other.Suit);
        }

        return result;
    }

    public bool Equals(Card other)
        => Rank.Equals(other.Rank) && Suit.Equals(other.Suit);

    public override bool Equals(object? o)
    {
        if (o is null || o.GetType() != GetType())
        {
            return false;
        }

        var c = (Card)o;
        return Rank == c.Rank && Suit == c.Suit;
    }

    public override int GetHashCode()
        => (Rank.GetHashCode(), Suit.GetHashCode()).GetHashCode();

    public override string ToString()
    {
        var rank = RankMapping[Rank];
        var suit = SuitMapping[Suit];
        return $"{rank}{suit}";
    }
}
