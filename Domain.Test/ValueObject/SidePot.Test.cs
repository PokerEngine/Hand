using Domain.ValueObject;

namespace Domain.Test.ValueObject;

public class SidePotTest
{
    [Fact]
    public void TestInitialization()
    {
        var competitors = new HashSet<Nickname> { "alice", "bobby" };
        var bets = new Bets()
            .Post("alice", new Chips(50))
            .Post("bobby", new Chips(50));
        var ante = new Chips(10);
        var sidePot = new SidePot(competitors, bets, ante);

        Assert.Equal(2, sidePot.Competitors.Count);
        Assert.Contains("alice", sidePot.Competitors);
        Assert.Contains("bobby", sidePot.Competitors);
        Assert.Equal(bets, sidePot.Bets);
        Assert.Equal(new Chips(10), sidePot.Ante);
    }

    [Fact]
    public void TestTotalAmount()
    {
        var competitors = new HashSet<Nickname> { "alice", "bobby" };
        var bets = new Bets()
            .Post("alice", new Chips(50))
            .Post("bobby", new Chips(50));
        var ante = new Chips(10);
        var sidePot = new SidePot(competitors, bets, ante);

        Assert.Equal(new Chips(110), sidePot.TotalAmount);
    }

    [Fact]
    public void TestEquals()
    {
        var competitors1 = new HashSet<Nickname> { "alice", "bobby" };
        var competitors2 = new HashSet<Nickname> { "alice", "charlie" };
        var bets1 = new Bets().Post("alice", new Chips(50));
        var bets2 = new Bets().Post("alice", new Chips(100));

        var sidePot1 = new SidePot(competitors1, bets1, new Chips(10));
        var sidePot2 = new SidePot(competitors1, bets1, new Chips(10));
        var sidePot3 = new SidePot(competitors2, bets1, new Chips(10));
        var sidePot4 = new SidePot(competitors1, bets2, new Chips(10));
        var sidePot5 = new SidePot(competitors1, bets1, new Chips(20));

        Assert.True(sidePot1.Equals(sidePot2));
        Assert.False(sidePot1.Equals(sidePot3));
        Assert.False(sidePot1.Equals(sidePot4));
        Assert.False(sidePot1.Equals(sidePot5));
    }

    [Fact]
    public void TestRepresentation()
    {
        var competitors = new HashSet<Nickname> { "alice", "bobby" };
        var bets = new Bets().Post("alice", new Chips(50));
        var sidePot = new SidePot(competitors, bets, new Chips(10));

        Assert.Equal("60 chip(s): {alice, bobby}", $"{sidePot}");
    }
}
