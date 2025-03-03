using Domain.Entity;
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

        Assert.Equal(cards, deck.Cards);
    }

    [Fact]
    public void TestCreate()
    {
        var deck = StandardDeck.Create();

        Assert.Equal(52, deck.Cards.Count);
    }

    [Fact]
    public void TestShuffle()
    {
        var deck = StandardDeck.Create();
        Assert.Equal(52, deck.Cards.Count);

        deck.Shuffle();

        Assert.Equal(52, deck.Cards.Count);
    }

    [Fact]
    public void TestExtract()
    {
        var deck = StandardDeck.Create();
        Assert.Equal(52, deck.Cards.Count);

        var cards = deck.Extract(2);

        Assert.Equal(50, deck.Cards.Count);
        Assert.Equal(new CardSet([Card.TreyOfClubs, Card.DeuceOfClubs]), cards);
    }

    [Fact]
    public void TestExtractWhenNotEnoughCards()
    {
        CardSet cards;
        var deck = new StandardDeck(new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]));
        Assert.Equal(2, deck.Cards.Count);

        var exc = Assert.Throws<NotAvailableError>(() => cards = deck.Extract(3));

        Assert.Equal("The deck does not contain enough cards", exc.Message);
    }

    [Fact]
    public void TestRepresentation()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);
        var deck = new StandardDeck(cards);

        Assert.Equal("StandardDeck: {AceOfSpades, DeuceOfClubs}", $"{deck}");
    }
}

public class ShortDeckTest
{
    [Fact]
    public void TestInitialization()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.SixOfClubs]);
        var deck = new ShortDeck(cards);

        Assert.Equal(cards, deck.Cards);
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
    public void TestCreate()
    {
        var deck = ShortDeck.Create();

        Assert.Equal(36, deck.Cards.Count);
    }

    [Fact]
    public void TestRepresentation()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.SixOfClubs]);
        var deck = new ShortDeck(cards);

        Assert.Equal("ShortDeck: {AceOfSpades, SixOfClubs}", $"{deck}");
    }
}