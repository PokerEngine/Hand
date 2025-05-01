using Domain.Error;
using Domain.ValueObject;

namespace Domain.Test.ValueObject;

public class SidePotTest
{
    [Fact]
    public void TestInitialization()
    {
        var nickname = new Nickname("Alpha");

        var sidePot = new SidePot();
        Assert.Equal(new Chips(0), sidePot.Get(nickname));
        Assert.Equal(new Chips(0), sidePot.Amount);
        Assert.Equal(0, sidePot.Count);
    }

    [Fact]
    public void TestAdd()
    {
        var nickname = new Nickname("Alpha");
        var sidePot = new SidePot();

        sidePot = sidePot.Add(nickname, new Chips(10));

        Assert.Equal(new Chips(10), sidePot.Get(nickname));
        Assert.Equal(new Chips(10), sidePot.Amount);
        Assert.Equal(1, sidePot.Count);

        sidePot = sidePot.Add(nickname, new Chips(20));

        Assert.Equal(new Chips(30), sidePot.Get(nickname));
        Assert.Equal(new Chips(30), sidePot.Amount);
        Assert.Equal(1, sidePot.Count);
    }

    [Fact]
    public void TestAddZeroAmount()
    {
        var nickname = new Nickname("Alpha");
        var sidePot = new SidePot();

        var exc = Assert.Throws<NotAvailableError>(() => sidePot.Add(nickname, new Chips(0)));

        Assert.Equal("Cannot add zero amount", exc.Message);
    }

    [Fact]
    public void TestSub()
    {
        var nickname = new Nickname("Alpha");
        var sidePot = new SidePot();

        sidePot = sidePot.Add(nickname, new Chips(30));
        sidePot = sidePot.Sub(nickname, new Chips(10));

        Assert.Equal(new Chips(20), sidePot.Get(nickname));
        Assert.Equal(new Chips(20), sidePot.Amount);
        Assert.Equal(1, sidePot.Count);

        sidePot = sidePot.Sub(nickname, new Chips(20));

        Assert.Equal(new Chips(0), sidePot.Get(nickname));
        Assert.Equal(new Chips(0), sidePot.Amount);
        Assert.Equal(0, sidePot.Count);
    }

    [Fact]
    public void TestSubZeroAmount()
    {
        var nickname = new Nickname("Alpha");
        var sidePot = new SidePot();

        sidePot = sidePot.Add(nickname, new Chips(30));
        var exc = Assert.Throws<NotAvailableError>(() => sidePot.Sub(nickname, new Chips(0)));

        Assert.Equal("Cannot sub zero amount", exc.Message);
    }

    [Fact]
    public void TestSubMoreThanAddedAmount()
    {
        var nickname = new Nickname("Alpha");
        var sidePot = new SidePot();

        sidePot = sidePot.Add(nickname, new Chips(30));
        var exc = Assert.Throws<NotAvailableError>(() => sidePot.Sub(nickname, new Chips(31)));

        Assert.Equal("Cannot sub more amount than added", exc.Message);
    }

    [Fact]
    public void TestMerge()
    {
        var nickname1 = new Nickname("Alpha");
        var nickname2 = new Nickname("Beta");
        var nickname3 = new Nickname("Gamma");
        var sidePot1 = new SidePot();
        var sidePot2 = new SidePot();

        sidePot1 = sidePot1.Add(nickname1, new Chips(30));
        sidePot1 = sidePot1.Add(nickname2, new Chips(10));
        sidePot2 = sidePot2.Add(nickname2, new Chips(25));
        sidePot2 = sidePot2.Add(nickname3, new Chips(15));

        var sidePot = sidePot1.Merge(sidePot2);

        Assert.Equal(new Chips(30), sidePot.Get(nickname1));
        Assert.Equal(new Chips(35), sidePot.Get(nickname2));
        Assert.Equal(new Chips(15), sidePot.Get(nickname3));
        Assert.Equal(new Chips(80), sidePot.Amount);
        Assert.Equal(3, sidePot.Count);
    }

    [Fact]
    public void TestEnumerator()
    {
        var nickname1 = new Nickname("Alpha");
        var nickname2 = new Nickname("Beta");
        var nickname3 = new Nickname("Gamma");
        var nickname4 = new Nickname("Delta");
        var sidePot = new SidePot();

        sidePot = sidePot.Add(nickname4, new Chips(10));
        sidePot = sidePot.Add(nickname1, new Chips(30));
        sidePot = sidePot.Add(nickname2, new Chips(10));
        sidePot = sidePot.Add(nickname3, new Chips(20));

        var items = new List<(Nickname, Chips)>();
        foreach (var (nickname, amount) in sidePot)
        {
            items.Add((nickname, amount));
        }

        // Ordered by (amount, nickname)
        Assert.Equal((nickname2, new Chips(10)), items[0]);
        Assert.Equal((nickname4, new Chips(10)), items[1]);
        Assert.Equal((nickname3, new Chips(20)), items[2]);
        Assert.Equal((nickname1, new Chips(30)), items[3]);
    }

    [Fact]
    public void TestRepresentation()
    {
        var nickname1 = new Nickname("Alpha");
        var nickname2 = new Nickname("Beta");
        var sidePot = new SidePot();

        sidePot = sidePot.Add(nickname1, new Chips(20));
        sidePot = sidePot.Add(nickname2, new Chips(10));

        Assert.Equal("30 chip(s), {Beta: 10 chip(s), Alpha: 20 chip(s)}", $"{sidePot}");
    }
}
