using Domain.ValueObject;

namespace Domain.Test.ValueObject;

public class CardTest
{
    [Fact]
    public void TestInitialization()
    {
        var card = Card.AceOfSpades;

        Assert.Equal(Rank.Ace, card.Rank);
        Assert.Equal(Suit.Spades, card.Suit);
    }

    [Fact]
    public void TestFromString()
    {
        Assert.Equal(Card.AceOfSpades, Card.FromString("As"));
    }

    [Fact]
    public void TestEquals()
    {
        Assert.True(Card.AceOfSpades.Equals(Card.AceOfSpades));
        Assert.False(Card.AceOfSpades.Equals(Card.AceOfHearts));
        Assert.False(Card.AceOfSpades.Equals(Card.KingOfSpades));
    }

    [Fact]
    public void TestCompareTo()
    {
        Assert.Equal(0, Card.AceOfSpades.CompareTo(Card.AceOfSpades));
        Assert.Equal(-1, Card.AceOfSpades.CompareTo(Card.KingOfSpades));
        Assert.Equal(-1, Card.AceOfSpades.CompareTo(Card.AceOfHearts));
        Assert.Equal(1, Card.KingOfSpades.CompareTo(Card.AceOfSpades));
        Assert.Equal(1, Card.AceOfHearts.CompareTo(Card.AceOfSpades));
    }

    [Fact]
    public void TestComparison()
    {
        Assert.True(Card.AceOfSpades == Card.AceOfSpades);
        Assert.False(Card.AceOfSpades == Card.AceOfHearts);
        Assert.False(Card.AceOfSpades == Card.KingOfSpades);
        Assert.False(Card.AceOfSpades != Card.AceOfSpades);
        Assert.True(Card.AceOfSpades != Card.AceOfHearts);
        Assert.True(Card.AceOfSpades != Card.KingOfSpades);
    }

    [Fact]
    public void TestRepresentation()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);
        Assert.Equal("As", $"{Card.AceOfSpades}");
    }
}
