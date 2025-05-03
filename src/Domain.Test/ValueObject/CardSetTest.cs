using Domain.ValueObject;

namespace Domain.Test.ValueObject;

public class CardSetTest
{
    [Fact]
    public void TestInitialization()
    {
        var items = new List<Card>() { Card.AceOfSpades, Card.DeuceOfClubs };
        var cards = new CardSet(items);
        Assert.Equal(items, cards.ToList());

        cards = new CardSet();
        Assert.Empty(cards.ToList());
    }

    [Fact]
    public void TestCount()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);
        Assert.Equal(2, cards.Count);

        cards = new CardSet();
        Assert.Empty(cards);
    }

    [Fact]
    public void TestEnumerator()
    {
        var items = new HashSet<Card> { Card.AceOfSpades, Card.DeuceOfClubs, Card.QueenOfDiamonds };
        var cards = new CardSet(items);

        var i = 0;
        foreach (var card in cards)
        {
            Assert.Contains(card, items);
            cards.Remove(card);
            i++;
        }

        Assert.Equal(3, i);
    }

    [Fact]
    public void TestContains()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        Assert.True(cards.Contains(Card.AceOfSpades));
        Assert.True(cards.Contains(Card.DeuceOfClubs));
        Assert.False(cards.Contains(Card.QueenOfDiamonds));
    }

    [Fact]
    public void TestIsSubsetOf()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        Assert.True(cards.IsSubsetOf(new List<Card>() { Card.AceOfSpades, Card.DeuceOfClubs }));
        Assert.True(cards.IsSubsetOf(new List<Card>() { Card.AceOfSpades, Card.DeuceOfClubs, Card.QueenOfDiamonds }));
        Assert.False(cards.IsSubsetOf(new List<Card>() { Card.AceOfSpades, Card.QueenOfDiamonds }));
    }

    [Fact]
    public void TestIsSupersetOf()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        Assert.True(cards.IsSupersetOf(new List<Card>() { Card.AceOfSpades, Card.DeuceOfClubs }));
        Assert.True(cards.IsSupersetOf(new List<Card>() { Card.AceOfSpades }));
        Assert.False(cards.IsSupersetOf(new List<Card>() { Card.AceOfSpades, Card.QueenOfDiamonds }));
    }

    [Fact]
    public void TestIsProperSubsetOf()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        Assert.True(cards.IsProperSubsetOf(new List<Card>() { Card.AceOfSpades, Card.DeuceOfClubs, Card.QueenOfDiamonds }));
        Assert.False(cards.IsProperSubsetOf(new List<Card>() { Card.AceOfSpades, Card.DeuceOfClubs }));
        Assert.False(cards.IsProperSubsetOf(new List<Card>() { Card.AceOfSpades, Card.QueenOfDiamonds }));
    }

    [Fact]
    public void TestIsProperSupersetOf()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        Assert.True(cards.IsProperSupersetOf(new List<Card>() { Card.AceOfSpades }));
        Assert.False(cards.IsProperSupersetOf(new List<Card>() { Card.AceOfSpades, Card.DeuceOfClubs }));
        Assert.False(cards.IsProperSupersetOf(new List<Card>() { Card.AceOfSpades, Card.QueenOfDiamonds }));
    }

    [Fact]
    public void TestOverlaps()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        Assert.True(cards.Overlaps(new List<Card>() { Card.AceOfSpades, Card.DeuceOfClubs }));
        Assert.True(cards.Overlaps(new List<Card>() { Card.AceOfSpades, Card.QueenOfDiamonds }));
        Assert.False(cards.Overlaps(new List<Card>() { Card.QueenOfDiamonds }));
    }

    [Fact]
    public void TestSetEquals()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        Assert.True(cards.SetEquals(new List<Card>() { Card.AceOfSpades, Card.DeuceOfClubs }));
        Assert.False(cards.SetEquals(new List<Card>() { Card.AceOfSpades, Card.DeuceOfClubs, Card.QueenOfDiamonds }));
        Assert.False(cards.SetEquals(new List<Card>() { Card.AceOfSpades, Card.QueenOfDiamonds }));
    }

    [Fact]
    public void TestAdd()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        Assert.Equal(new CardSet([Card.AceOfSpades, Card.JackOfHearts, Card.DeuceOfClubs]), cards.Add(Card.JackOfHearts));
    }

    [Fact]
    public void TestRemove()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        Assert.Equal(new CardSet([Card.DeuceOfClubs]), cards.Remove(Card.AceOfSpades));
    }

    [Fact]
    public void TestExcept()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.JackOfClubs, Card.DeuceOfClubs]);

        Assert.Equal(new CardSet([Card.DeuceOfClubs]), cards.Except(new CardSet([Card.AceOfSpades, Card.JackOfClubs])));
    }

    [Fact]
    public void TestComparison()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        Assert.True(cards == new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]));
        Assert.True(cards == new CardSet([Card.DeuceOfClubs, Card.AceOfSpades]));
        Assert.False(cards == new CardSet([Card.AceOfSpades, Card.QueenOfDiamonds]));
        Assert.False(cards != new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]));
        Assert.False(cards != new CardSet([Card.DeuceOfClubs, Card.AceOfSpades]));
        Assert.True(cards != new CardSet([Card.AceOfSpades, Card.QueenOfDiamonds]));
    }

    [Fact]
    public void TestAddition()
    {
        var cards1 = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);
        var cards2 = new CardSet([Card.QueenOfDiamonds]);
        var expected = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs, Card.QueenOfDiamonds]);

        Assert.Equal(cards1 + cards2, expected);
    }

    [Fact]
    public void TestRepresentation()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);
        Assert.Equal("{As, 2c}", $"{cards}");
    }
}
