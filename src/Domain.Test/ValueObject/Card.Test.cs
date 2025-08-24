using Domain.ValueObject;

namespace Domain.Test.ValueObject;

public class CardTest
{
    [Fact]
    public void TestFromString()
    {
        Assert.Equal(Card.AceOfSpades, Card.FromString("As"));
    }

    [Fact]
    public void TestRandAndSuit()
    {
        Assert.Equal(Rank.Ace, Card.AceOfSpades.Rank);
        Assert.Equal(Suit.Spades, Card.AceOfSpades.Suit);
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
        Assert.False(Card.AceOfSpades == Card.AceOfHearts);
        Assert.False(Card.AceOfSpades == Card.KingOfSpades);
        Assert.True(Card.AceOfSpades != Card.AceOfHearts);
        Assert.True(Card.AceOfSpades != Card.KingOfSpades);
    }

    [Fact]
    public void TestRepresentation()
    {
        Assert.Equal("As", $"{Card.AceOfSpades}");
    }
}
