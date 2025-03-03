using Domain.ValueObject;

namespace DomainTest.ValueObjectTest;

public class CardSetTest
{
    [Fact]
    public void TestInitialization()
    {
        var items = new List<Card>() {Card.AceOfSpades, Card.DeuceOfClubs};
        var cards = new CardSet(items);
        Assert.Equal(items, cards.ToList());

        cards = new CardSet();
        Assert.Empty(cards.ToList());
    }

    [Fact]
    public void TestInitializationWithDuplicates()
    {
        var items = new List<Card>() {Card.AceOfSpades, Card.AceOfSpades, Card.DeuceOfClubs};
        CardSet cards;

        var exc = Assert.Throws<ArgumentException>(() => cards = new CardSet(items));
        Assert.Equal("CardSet cannot contain duplicates", exc.Message);
    }

    [Fact]
    public void TestCount()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);
        Assert.Equal(2, cards.Count);

        cards = new CardSet();
        Assert.Equal(0, cards.Count);
    }

    [Fact]
    public void TestIndex()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);
        Assert.Equal(Card.AceOfSpades, cards[0]);
        Assert.Equal(Card.DeuceOfClubs, cards[1]);
    }

    [Fact]
    public void TestIndexOutOfRange()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);
        Assert.Throws<ArgumentOutOfRangeException>(() => cards[3]);
        Assert.Throws<ArgumentOutOfRangeException>(() => cards[-1]);

        cards = new CardSet();
        Assert.Throws<ArgumentOutOfRangeException>(() => cards[0]);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(0, 2)]
    [InlineData(1, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 2)]
    public void TestSlice(int start, int end)
    {
        var items = new List<Card>() {Card.AceOfSpades, Card.DeuceOfClubs};
        var cards = new CardSet(items);

        Assert.Equal(items[start..end].ToList(), cards[start..end].ToList());
    }

    [Fact]
    public void TestSliceOutOfRange()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        Assert.Throws<ArgumentOutOfRangeException>(() => cards[-1..1]);
        Assert.Throws<ArgumentOutOfRangeException>(() => cards[1..10]);
        Assert.Throws<OverflowException>(() => cards[2..1]);
    }

    [Fact]
    public void TestEnumerator()
    {
        var items = new List<Card>() {Card.AceOfSpades, Card.DeuceOfClubs, Card.QueenOfDiamonds};
        var cards = new CardSet(items);

        var i = 0;
        foreach (var card in cards)
        {
            Assert.Equal(card, items[i]);
            i++;
        }
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

        Assert.True(cards.IsSubsetOf(new List<Card>() {Card.AceOfSpades, Card.DeuceOfClubs}));
        Assert.True(cards.IsSubsetOf(new List<Card>() {Card.AceOfSpades, Card.DeuceOfClubs, Card.QueenOfDiamonds}));
        Assert.False(cards.IsSubsetOf(new List<Card>() {Card.AceOfSpades, Card.QueenOfDiamonds}));
    }

    [Fact]
    public void TestIsSupersetOf()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        Assert.True(cards.IsSupersetOf(new List<Card>() {Card.AceOfSpades, Card.DeuceOfClubs}));
        Assert.True(cards.IsSupersetOf(new List<Card>() {Card.AceOfSpades}));
        Assert.False(cards.IsSupersetOf(new List<Card>() {Card.AceOfSpades, Card.QueenOfDiamonds}));
    }

    [Fact]
    public void TestIsProperSubsetOf()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        Assert.True(cards.IsProperSubsetOf(new List<Card>() {Card.AceOfSpades, Card.DeuceOfClubs, Card.QueenOfDiamonds}));
        Assert.False(cards.IsProperSubsetOf(new List<Card>() {Card.AceOfSpades, Card.DeuceOfClubs}));
        Assert.False(cards.IsProperSubsetOf(new List<Card>() {Card.AceOfSpades, Card.QueenOfDiamonds}));
    }

    [Fact]
    public void TestIsProperSupersetOf()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        Assert.True(cards.IsProperSupersetOf(new List<Card>() {Card.AceOfSpades}));
        Assert.False(cards.IsProperSupersetOf(new List<Card>() {Card.AceOfSpades, Card.DeuceOfClubs}));
        Assert.False(cards.IsProperSupersetOf(new List<Card>() {Card.AceOfSpades, Card.QueenOfDiamonds}));
    }

    [Fact]
    public void TestOverlaps()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        Assert.True(cards.Overlaps(new List<Card>() {Card.AceOfSpades, Card.DeuceOfClubs}));
        Assert.True(cards.Overlaps(new List<Card>() {Card.AceOfSpades, Card.QueenOfDiamonds}));
        Assert.False(cards.Overlaps(new List<Card>() {Card.QueenOfDiamonds}));
    }

    [Fact]
    public void TestSetEquals()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        Assert.True(cards.SetEquals(new List<Card>() {Card.AceOfSpades, Card.DeuceOfClubs}));
        Assert.False(cards.SetEquals(new List<Card>() {Card.AceOfSpades, Card.DeuceOfClubs, Card.QueenOfDiamonds}));
        Assert.False(cards.SetEquals(new List<Card>() {Card.AceOfSpades, Card.QueenOfDiamonds}));
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
        Assert.Equal("{AceOfSpades, DeuceOfClubs}", $"{cards}");
    }
}