using Domain.Error;
using Domain.ValueObject;

namespace Domain.Entity;

public abstract class BaseDeck
{
    public CardSet Cards { get; private set; }
    private static readonly Random Rand = new ();

    protected BaseDeck(CardSet cards)
    {
        Cards = cards;
    }

    public void Shuffle()
    {
        Cards = new CardSet(Cards.ToList().OrderBy(_ => Rand.Next()));
    }

    public CardSet Extract(int count)
    {
        if (count > Cards.Count)
        {
            throw new NotAvailableError("The deck does not contain enough cards");
        }

        var index = Cards.Count - count;
        var extracted = Cards[index..Cards.Count];
        var remaining = Cards[0..index];
        Cards = remaining;
        return extracted;
    }

    public override string ToString()
        => $"{GetType().Name}: {Cards}";
}

public class StandardDeck : BaseDeck
{
    public static readonly CardSet AllowedCards = new ([
        Card.AceOfSpades,
        Card.KingOfSpades,
        Card.QueenOfSpades,
        Card.JackOfSpades,
        Card.TenOfSpades,
        Card.NineOfSpades,
        Card.EightOfSpades,
        Card.SevenOfSpades,
        Card.SixOfSpades,
        Card.FiveOfSpades,
        Card.FourOfSpades,
        Card.TreyOfSpades,
        Card.DeuceOfSpades,
        Card.AceOfHearts,
        Card.KingOfHearts,
        Card.QueenOfHearts,
        Card.JackOfHearts,
        Card.TenOfHearts,
        Card.NineOfHearts,
        Card.EightOfHearts,
        Card.SevenOfHearts,
        Card.SixOfHearts,
        Card.FiveOfHearts,
        Card.FourOfHearts,
        Card.TreyOfHearts,
        Card.DeuceOfHearts,
        Card.AceOfDiamonds,
        Card.KingOfDiamonds,
        Card.QueenOfDiamonds,
        Card.JackOfDiamonds,
        Card.TenOfDiamonds,
        Card.NineOfDiamonds,
        Card.EightOfDiamonds,
        Card.SevenOfDiamonds,
        Card.SixOfDiamonds,
        Card.FiveOfDiamonds,
        Card.FourOfDiamonds,
        Card.TreyOfDiamonds,
        Card.DeuceOfDiamonds,
        Card.AceOfClubs,
        Card.KingOfClubs,
        Card.QueenOfClubs,
        Card.JackOfClubs,
        Card.TenOfClubs,
        Card.NineOfClubs,
        Card.EightOfClubs,
        Card.SevenOfClubs,
        Card.SixOfClubs,
        Card.FiveOfClubs,
        Card.FourOfClubs,
        Card.TreyOfClubs,
        Card.DeuceOfClubs,
    ]);

    public StandardDeck(CardSet cards) : base(cards)
    {
        if (!cards.IsSubsetOf(AllowedCards))
        {
            throw new NotAvailableError("The deck must contain allowed cards");
        }
    }
}

public class ShortDeck : BaseDeck
{
    public static readonly CardSet AllowedCards = new ([
        Card.AceOfSpades,
        Card.KingOfSpades,
        Card.QueenOfSpades,
        Card.JackOfSpades,
        Card.TenOfSpades,
        Card.NineOfSpades,
        Card.EightOfSpades,
        Card.SevenOfSpades,
        Card.SixOfSpades,
        Card.AceOfHearts,
        Card.KingOfHearts,
        Card.QueenOfHearts,
        Card.JackOfHearts,
        Card.TenOfHearts,
        Card.NineOfHearts,
        Card.EightOfHearts,
        Card.SevenOfHearts,
        Card.SixOfHearts,
        Card.AceOfDiamonds,
        Card.KingOfDiamonds,
        Card.QueenOfDiamonds,
        Card.JackOfDiamonds,
        Card.TenOfDiamonds,
        Card.NineOfDiamonds,
        Card.EightOfDiamonds,
        Card.SevenOfDiamonds,
        Card.SixOfDiamonds,
        Card.AceOfClubs,
        Card.KingOfClubs,
        Card.QueenOfClubs,
        Card.JackOfClubs,
        Card.TenOfClubs,
        Card.NineOfClubs,
        Card.EightOfClubs,
        Card.SevenOfClubs,
        Card.SixOfClubs,
    ]);

    public ShortDeck(CardSet cards) : base(cards)
    {
        if (!cards.IsSubsetOf(AllowedCards))
        {
            throw new NotAvailableError("The deck must contain allowed cards");
        }
    }
}