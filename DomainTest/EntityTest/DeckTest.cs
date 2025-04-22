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
        var deck = new StandardDeck();

        Assert.Equal(52, deck.Count);
    }

    [Fact]
    public void TestExtractRandomCards()
    {
        var deck = new StandardDeck();

        var extractedCards = deck.ExtractRandomCards(1);

        Assert.Equal(51, deck.Count);
        Assert.Single(extractedCards);

        extractedCards = deck.ExtractRandomCards(2);

        Assert.Equal(49, deck.Count);
        Assert.Equal(2, extractedCards.Count);
    }

    [Fact]
    public void TestExtractRandomCardsWhenNotEnoughCards()
    {
        var deck = new StandardDeck();

        CardSet cards;
        var exc = Assert.Throws<NotAvailableError>(() => cards = deck.ExtractRandomCards(53));

        Assert.Equal("The deck does not contain enough cards", exc.Message);
    }

    [Fact]
    public void TestExtractCertainCards()
    {
        var deck = new StandardDeck();

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
        var deck = new StandardDeck();
        deck.ExtractCertainCards(new CardSet([Card.AceOfDiamonds, Card.DeuceOfClubs]));

        CardSet cards;
        var exc = Assert.Throws<NotAvailableError>(() => cards = deck.ExtractCertainCards(new CardSet([Card.AceOfDiamonds])));

        Assert.Equal("The deck does not contain the given cards", exc.Message);
    }

    [Fact]
    public void TestRepresentation()
    {
        var deck = new StandardDeck();

        Assert.Equal("StandardDeck: 52 card(s)", $"{deck}");
    }
}

public class ShortDeckTest
{
    [Fact]
    public void TestInitialization()
    {
        var deck = new ShortDeck();

        Assert.Equal(36, deck.Count);
    }

    [Fact]
    public void TestRepresentation()
    {
        var deck = new ShortDeck();

        Assert.Equal("ShortDeck: 36 card(s)", $"{deck}");
    }
}