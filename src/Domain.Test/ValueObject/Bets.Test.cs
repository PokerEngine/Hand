using Domain.ValueObject;

namespace Domain.Test.ValueObject;

public class BetsTest
{
    [Fact]
    public void TestInitialization()
    {
        var bets = new Bets();
        Assert.Equal(Chips.Zero, bets.TotalAmount);
    }

    [Fact]
    public void TestTotalAmount()
    {
        var bets = new Bets()
            .Post("alice", new Chips(50))
            .Post("bobby", new Chips(75));

        Assert.Equal(new Chips(125), bets.TotalAmount);
    }

    [Fact]
    public void TestAddition()
    {
        var bets1 = new Bets()
            .Post("alice", new Chips(50))
            .Post("bobby", new Chips(30));

        var bets2 = new Bets()
            .Post("alice", new Chips(25))
            .Post("charlie", new Chips(40));

        var result = bets1 + bets2;

        Assert.Equal(new Chips(75), result.GetAmountPostedBy("alice"));
        Assert.Equal(new Chips(30), result.GetAmountPostedBy("bobby"));
        Assert.Equal(new Chips(40), result.GetAmountPostedBy("charlie"));
        Assert.Equal(new Chips(145), result.TotalAmount);
    }

    [Fact]
    public void TestSubtraction()
    {
        var bets1 = new Bets()
            .Post("alice", new Chips(100))
            .Post("bobby", new Chips(75));

        var bets2 = new Bets()
            .Post("alice", new Chips(25))
            .Post("bobby", new Chips(50));

        var result = bets1 - bets2;

        Assert.Equal(new Chips(75), result.GetAmountPostedBy("alice"));
        Assert.Equal(new Chips(25), result.GetAmountPostedBy("bobby"));
        Assert.Equal(new Chips(100), result.TotalAmount);
    }

    [Fact]
    public void TestGetAmountPostedBy()
    {
        var bets = new Bets()
            .Post("alice", new Chips(50))
            .Post("bobby", new Chips(75));

        Assert.Equal(new Chips(50), bets.GetAmountPostedBy("alice"));
        Assert.Equal(new Chips(75), bets.GetAmountPostedBy("bobby"));
        Assert.Equal(Chips.Zero, bets.GetAmountPostedBy("charlie"));
    }

    [Fact]
    public void TestGetMaxAmountPostedNotBy()
    {
        var bets = new Bets()
            .Post("alice", new Chips(50))
            .Post("bobby", new Chips(100))
            .Post("charlie", new Chips(75));

        Assert.Equal(new Chips(100), bets.GetMaxAmountPostedNotBy("alice"));
        Assert.Equal(new Chips(100), bets.GetMaxAmountPostedNotBy("charlie"));
        Assert.Equal(new Chips(75), bets.GetMaxAmountPostedNotBy("bobby"));
    }

    [Fact]
    public void TestGetMaxAmountPostedNotByWhenEmpty()
    {
        var bets = new Bets();
        Assert.Equal(Chips.Zero, bets.GetMaxAmountPostedNotBy("alice"));
    }

    [Fact]
    public void TestGetMaxAmountPostedNotByWhenOnlyOnePlayer()
    {
        var bets = new Bets().Post("alice", new Chips(50));
        Assert.Equal(Chips.Zero, bets.GetMaxAmountPostedNotBy("alice"));
    }

    [Fact]
    public void TestGetNicknamePostedMaxAmount()
    {
        var bets = new Bets()
            .Post("alice", new Chips(50))
            .Post("bobby", new Chips(100))
            .Post("charlie", new Chips(75));

        Assert.Equal("bobby", bets.GetNicknamePostedMaxAmount());
    }

    [Fact]
    public void TestGetNicknamePostedMaxAmountWhenEmpty()
    {
        var bets = new Bets();
        Assert.Null(bets.GetNicknamePostedMaxAmount());
    }

    [Fact]
    public void TestPost()
    {
        var bets = new Bets().Post("alice", new Chips(50));
        Assert.Equal(new Chips(50), bets.GetAmountPostedBy("alice"));
        Assert.Equal(new Chips(50), bets.TotalAmount);
    }

    [Fact]
    public void TestPostMultipleTimes()
    {
        var bets = new Bets()
            .Post("alice", new Chips(50))
            .Post("alice", new Chips(30));

        Assert.Equal(new Chips(80), bets.GetAmountPostedBy("alice"));
        Assert.Equal(new Chips(80), bets.TotalAmount);
    }

    [Fact]
    public void TestRefund()
    {
        var bets = new Bets()
            .Post("alice", new Chips(100))
            .Refund("alice", new Chips(30));

        Assert.Equal(new Chips(70), bets.GetAmountPostedBy("alice"));
        Assert.Equal(new Chips(70), bets.TotalAmount);
    }

    [Fact]
    public void TestRefundFullAmount()
    {
        var bets = new Bets()
            .Post("alice", new Chips(100))
            .Refund("alice", new Chips(100));

        Assert.Equal(Chips.Zero, bets.GetAmountPostedBy("alice"));
        Assert.Equal(Chips.Zero, bets.TotalAmount);
    }

    [Fact]
    public void TestRefundMoreThanPosted()
    {
        var bets = new Bets().Post("alice", new Chips(50));

        var exc = Assert.Throws<InvalidOperationException>(() => bets.Refund("alice", new Chips(75)));
        Assert.Equal("Cannot refund more than posted", exc.Message);
    }

    [Fact]
    public void TestEnumeration()
    {
        var bets = new Bets()
            .Post("alice", new Chips(50))
            .Post("bobby", new Chips(100))
            .Post("charlie", new Chips(50));

        var list = bets.ToList();
        Assert.Equal(3, list.Count);

        Assert.Equal("alice", list[0].Key);
        Assert.Equal(new Chips(50), list[0].Value);

        Assert.Equal("charlie", list[1].Key);
        Assert.Equal(new Chips(50), list[1].Value);

        Assert.Equal("bobby", list[2].Key);
        Assert.Equal(new Chips(100), list[2].Value);
    }

    [Fact]
    public void TestRepresentation()
    {
        var bets = new Bets().Post("alice", new Chips(50));
        Assert.Equal("{alice: 50 chip(s)}", $"{bets}");
    }
}
