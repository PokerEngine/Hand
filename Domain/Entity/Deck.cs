using Domain.Error;
using Domain.ValueObject;

namespace Domain.Entity;

public abstract class BaseDeck
{
    private CardSet _cards;
    private static readonly Random Rand = new ();

    public int Count => _cards.Count;

    protected BaseDeck(CardSet cards)
    {
        _cards = cards;
    }

    public void Shuffle()
    {
        _cards = new CardSet(_cards.ToList().OrderBy(_ => Rand.Next()));
    }

    public CardSet Extract(int count)
    {
        if (count > _cards.Count)
        {
            throw new NotAvailableError("The deck does not contain enough cards");
        }

        var index = _cards.Count - count;
        var extracted = _cards[index.._cards.Count];
        var remaining = _cards[..index];
        _cards = remaining;
        return extracted;
    }

    public override string ToString()
        => $"{GetType().Name}: {_cards}";
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