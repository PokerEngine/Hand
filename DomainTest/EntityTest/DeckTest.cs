using Domain.Entity;
using Domain.Entity.Factory;
using Domain.Error;
using Domain.ValueObject;

namespace DomainTest.EntityTest;

public class StandardDeckTest
{
    [Fact]
    public void TestInitialization()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);
        var deck = new StandardDeck(cards);

        Assert.Equal(2, deck.Count);
    }

    [Fact]
    public void TestExtractRandomCards()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs, Card.JackOfDiamonds]);
        var deck = new StandardDeck(cards);
        Assert.Equal(3, deck.Count);

        var extractedCards = deck.ExtractRandomCards(1);

        Assert.Equal(2, deck.Count);
        Assert.Equal(1, extractedCards.Count);
        Assert.True(extractedCards.IsSubsetOf(cards));

        extractedCards = deck.ExtractRandomCards(2);

        Assert.Equal(0, deck.Count);
        Assert.Equal(2, extractedCards.Count);
        Assert.True(extractedCards.IsSubsetOf(cards));
    }

    [Fact]
    public void TestExtractRandomCardsWhenNotEnoughCards()
    {
        CardSet cards;
        var deck = new StandardDeck(new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]));
        Assert.Equal(2, deck.Count);

        var exc = Assert.Throws<NotAvailableError>(() => cards = deck.ExtractRandomCards(3));

        Assert.Equal("The deck does not contain enough cards", exc.Message);
    }

    [Fact]
    public void TestExtractCertainCards()
    {
        var deck = new StandardDeck(StandardDeck.AllowedCards);
        Assert.Equal(52, deck.Count);

        var cards = new CardSet([Card.AceOfSpades]);
        var extractedCards = deck.ExtractCertainCards(cards);

        Assert.Equal(51, deck.Count);
        Assert.Equal(cards, extractedCards);

        cards = new CardSet([Card.DeuceOfClubs, Card.JackOfDiamonds]);
        extractedCards = deck.ExtractCertainCards(cards);

        Assert.Equal(49, deck.Count);
        Assert.Equal(cards, extractedCards);
    }

    [Fact]
    public void TestExtractCertainCardsWhenDoesNotContain()
    {
        CardSet cards;
        var deck = new StandardDeck(new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]));
        Assert.Equal(2, deck.Count);

        var exc = Assert.Throws<NotAvailableError>(() => cards = deck.ExtractCertainCards(new CardSet([Card.AceOfDiamonds])));

        Assert.Equal("The deck does not contain the given cards", exc.Message);
    }

    [Fact]
    public void TestRepresentation()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);
        var deck = new StandardDeck(cards);

        Assert.Equal("StandardDeck: 2 card(s)", $"{deck}");
    }
}

public class ShortDeckTest
{
    [Fact]
    public void TestInitialization()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.SixOfClubs]);
        var deck = new ShortDeck(cards);

        Assert.Equal(2, deck.Count);
    }

    [Fact]
    public void TestInitializationWithNotAllowedCards()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        ShortDeck deck;
        var exc = Assert.Throws<NotAvailableError>(() => deck = new ShortDeck(cards));
        Assert.Equal("The deck must contain allowed cards", exc.Message);
    }

    [Fact]
    public void TestRepresentation()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.SixOfClubs]);
        var deck = new ShortDeck(cards);

        Assert.Equal("ShortDeck: 2 card(s)", $"{deck}");
    }
}