using Domain.Exception;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Domain.Entity;

public abstract class BaseDeck
{
    protected CardSet Cards;

    public int Count => Cards.Count;

    public CardSet ExtractRandomCards(int count, IRandomizer randomizer)
    {
        if (count > Cards.Count)
        {
            throw new InvalidHandStateException("The deck does not contain enough cards");
        }

        var extractedCards = new CardSet();
        for (var i = 0; i < count; i++)
        {
            var idx = randomizer.GetRandomNumber(Cards.Count - 1);
            var card = Cards.ToList()[idx];
            Cards = Cards.Remove(card);
            extractedCards = extractedCards.Add(card);
        }
        return extractedCards;
    }

    public CardSet ExtractCertainCards(CardSet cards)
    {
        if (!cards.IsSubsetOf(Cards))
        {
            throw new InvalidHandStateException("The deck does not contain the given cards");
        }

        Cards = Cards.Except(cards);
        return cards;
    }

    public override string ToString()
        => $"{GetType().Name}: {Cards.Count} card(s)";
}

public class StandardDeck : BaseDeck
{
    private static readonly CardSet AllowedCards = new([
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

    public StandardDeck()
    {
        Cards = AllowedCards;
    }
}

public class ShortDeck : BaseDeck
{
    private static readonly CardSet AllowedCards = new([
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

    public ShortDeck()
    {
        Cards = AllowedCards;
    }
}
