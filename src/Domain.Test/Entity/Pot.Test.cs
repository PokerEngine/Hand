using Domain.Entity;
using Domain.ValueObject;

namespace Domain.Test.Entity;

public class PotTest
{
    [Fact]
    public void Constructor_ShouldConstruct()
    {
        // Arrange & Act
        var pot = new Pot(
            minBet: new Chips(10)
        );

        // Assert
        Assert.Equal(new Chips(10), pot.MinBet);
        Assert.Equal(new Chips(0), pot.TotalAmount);
        Assert.Null(pot.LastPostedNickname);
        Assert.Null(pot.LastRaisedNickname);
        Assert.Equal(new Chips(10), pot.LastRaisedStep);
    }

    [Fact]
    public void PostAnte_ShouldPost()
    {
        // Arrange
        var pot = CreatePot();

        // Act
        pot.PostAnte(new Chips(5));
        pot.PostAnte(new Chips(4));

        // Assert
        Assert.Equal(new Chips(9), pot.TotalAmount);
        Assert.Null(pot.LastPostedNickname);
        Assert.Null(pot.LastRaisedNickname);
        Assert.Equal(new Chips(10), pot.LastRaisedStep);
    }

    [Fact]
    public void PostSmallBlind_ShouldPostAndKeepLastRaiseStep()
    {
        // Arrange
        var pot = CreatePot();
        var nickname = new Nickname("Alice");

        // Act
        pot.PostBlind(nickname, new Chips(5));

        // Assert
        Assert.Equal(new Chips(5), pot.TotalAmount);
        Assert.Equal(nickname, pot.LastPostedNickname);
        Assert.Equal(nickname, pot.LastRaisedNickname);
        Assert.Equal(new Chips(10), pot.LastRaisedStep);
        Assert.False(pot.PostedUncommittedBet(nickname));
    }

    [Fact]
    public void PostBigBlindBlind_ShouldPostAndKeepLastRaiseStep()
    {
        // Arrange
        var pot = CreatePot();
        var nicknameA = new Nickname("Alice");
        var nicknameB = new Nickname("Bobby");
        pot.PostBlind(nicknameA, new Chips(5));

        // Act
        pot.PostBlind(nicknameB, new Chips(10));

        // Assert
        Assert.Equal(new Chips(15), pot.TotalAmount);
        Assert.Equal(nicknameB, pot.LastPostedNickname);
        Assert.Equal(nicknameB, pot.LastRaisedNickname);
        Assert.Equal(new Chips(10), pot.LastRaisedStep);
        Assert.False(pot.PostedUncommittedBet(nicknameB));
    }

    [Fact]
    public void PostStraddle_ShouldPostAndUpdateLastRaiseStep()
    {
        // Arrange
        var pot = CreatePot();
        var nicknameA = new Nickname("Alice");
        var nicknameB = new Nickname("Bobby");
        var nicknameC = new Nickname("Charlie");
        pot.PostBlind(nicknameA, new Chips(5));
        pot.PostBlind(nicknameB, new Chips(10));

        // Act
        pot.PostBlind(nicknameC, new Chips(20));

        // Assert
        Assert.Equal(new Chips(35), pot.TotalAmount);
        Assert.Equal(nicknameC, pot.LastPostedNickname);
        Assert.Equal(nicknameC, pot.LastRaisedNickname);
        Assert.Equal(new Chips(20), pot.LastRaisedStep);
        Assert.False(pot.PostedUncommittedBet(nicknameC));
    }

    [Fact]
    public void PostBet_WhenBets_ShouldPostAndUpdateLastRaise()
    {
        // Arrange
        var pot = CreatePot();
        var nickname = new Nickname("Alice");

        // Act
        pot.PostBet(nickname, new Chips(20));

        // Assert
        Assert.Equal(new Chips(20), pot.TotalAmount);
        Assert.Equal(nickname, pot.LastPostedNickname);
        Assert.Equal(nickname, pot.LastRaisedNickname);
        Assert.Equal(new Chips(20), pot.LastRaisedStep);
        Assert.True(pot.PostedUncommittedBet(nickname));
    }

    [Theory]
    [InlineData(20)]
    [InlineData(15)] // All-in, not enough to call
    public void PostBet_WhenCalls_ShouldPostAndKeepLastRaise(int amount)
    {
        // Arrange
        var pot = CreatePot();
        var nicknameA = new Nickname("Alice");
        var nicknameB = new Nickname("Bobby");
        pot.PostBet(nicknameA, new Chips(20));

        // Act
        pot.PostBet(nicknameB, new Chips(amount));

        // Assert
        Assert.Equal(new Chips(20 + amount), pot.TotalAmount);
        Assert.Equal(nicknameB, pot.LastPostedNickname);
        Assert.Equal(nicknameA, pot.LastRaisedNickname);
        Assert.Equal(new Chips(20), pot.LastRaisedStep);
    }

    [Theory]
    [InlineData(40)] // Min raise
    [InlineData(45)]
    public void PostBet_WhenRaises_ShouldPostAndUpdateLastRaise(int amount)
    {
        // Arrange
        var pot = CreatePot();
        var nicknameA = new Nickname("Alice");
        var nicknameB = new Nickname("Bobby");
        pot.PostBet(nicknameA, new Chips(20));

        // Act
        pot.PostBet(nicknameB, new Chips(amount));

        // Assert
        Assert.Equal(new Chips(20 + amount), pot.TotalAmount);
        Assert.Equal(nicknameB, pot.LastPostedNickname);
        Assert.Equal(nicknameB, pot.LastRaisedNickname);
        Assert.Equal(new Chips(amount - 20), pot.LastRaisedStep);
    }

    [Fact]
    public void PostBet_WhenAllIns_ShouldPostAndKeepLastRaise()
    {
        // Arrange
        var pot = CreatePot();
        var nicknameA = new Nickname("Alice");
        var nicknameB = new Nickname("Bobby");
        pot.PostBet(nicknameA, new Chips(20));

        // Act
        pot.PostBet(nicknameB, new Chips(25));

        // Assert
        Assert.Equal(new Chips(45), pot.TotalAmount);
        Assert.Equal(nicknameB, pot.LastPostedNickname);
        Assert.Equal(nicknameA, pot.LastRaisedNickname);
        Assert.Equal(new Chips(20), pot.LastRaisedStep);
    }

    [Fact]
    public void PostBet_When3bets_ShouldPostAndUpdateLastRaise()
    {
        // Arrange
        var pot = CreatePot();
        var nicknameA = new Nickname("Alice");
        var nicknameB = new Nickname("Bobby");
        pot.PostBlind(nicknameA, new Chips(5));
        pot.PostBlind(nicknameB, new Chips(10));
        pot.PostBet(nicknameA, new Chips(20)); // Open raise 25

        // Act
        pot.PostBet(nicknameB, new Chips(90)); // 3bet 100

        // Assert
        Assert.Equal(new Chips(125), pot.TotalAmount);
        Assert.Equal(nicknameB, pot.LastPostedNickname);
        Assert.Equal(nicknameB, pot.LastRaisedNickname);
        Assert.Equal(new Chips(75), pot.LastRaisedStep);
    }

    [Fact]
    public void RefundBet_ShouldRefundBet()
    {
        // Arrange
        var pot = CreatePot();
        var nicknameA = new Nickname("Alice");
        var nicknameB = new Nickname("Bobby");
        pot.PostBlind(nicknameA, new Chips(5));
        pot.PostBlind(nicknameB, new Chips(10));
        pot.PostBet(nicknameA, new Chips(20)); // Raise 25

        // Act
        pot.RefundBet(nicknameA, new Chips(15));

        // Assert
        Assert.Equal(new Chips(20), pot.TotalAmount);
        Assert.Equal(new Chips(10), pot.GetUncommittedAmountPostedBy(nicknameA));
    }

    [Fact]
    public void CommitBets_ShouldCommitBets()
    {
        // Arrange
        var pot = CreatePot();
        var nicknameA = new Nickname("Alice");
        var nicknameB = new Nickname("Bobby");
        pot.PostBlind(nicknameA, new Chips(5));
        pot.PostBlind(nicknameB, new Chips(10));
        pot.PostBet(nicknameA, new Chips(20)); // Raise 25
        pot.PostBet(nicknameB, new Chips(15)); // Call

        // Act
        pot.CommitBets();

        // Assert
        Assert.Equal(new Chips(50), pot.TotalAmount);
        Assert.Null(pot.LastPostedNickname);
        Assert.Null(pot.LastRaisedNickname);
        Assert.Equal(new Chips(10), pot.LastRaisedStep);
        Assert.False(pot.PostedUncommittedBet(nicknameA));
        Assert.False(pot.PostedUncommittedBet(nicknameB));
    }

    [Fact]
    public void WinSidePot_ShouldWin()
    {
        // Arrange
        var pot = CreatePot();
        var nicknameA = new Nickname("Alice");
        var nicknameB = new Nickname("Bobby");
        pot.PostBlind(nicknameA, new Chips(5));
        pot.PostBlind(nicknameB, new Chips(10));
        pot.PostBet(nicknameA, new Chips(20)); // Raise 25
        pot.RefundBet(nicknameA, new Chips(15));
        pot.CommitBets();

        // Act
        var bets = new Bets()
            .Post(nicknameA, new Chips(10))
            .Post(nicknameB, new Chips(10));
        var sidePot = new SidePot([nicknameA], bets, Chips.Zero);
        pot.WinSidePot(sidePot, [nicknameA]);

        // Assert
        Assert.Equal(Chips.Zero, pot.TotalAmount);
    }

    [Fact]
    public void CalculateRefund_WhenAvailable_ShouldReturnRefund()
    {
        // Arrange
        var pot = CreatePot();
        var nicknameA = new Nickname("Alice");
        var nicknameB = new Nickname("Bobby");
        pot.PostBlind(nicknameA, new Chips(5));
        pot.PostBlind(nicknameB, new Chips(10));
        pot.PostBet(nicknameA, new Chips(20)); // Raise 25

        // Act
        var (nickname, amount) = pot.CalculateRefund();

        // Assert
        Assert.Equal(nicknameA, nickname);
        Assert.Equal(new Chips(15), amount);
    }

    [Fact]
    public void CalculateRefund_WhenNotAvailable_ShouldReturnNull()
    {
        // Arrange
        var pot = CreatePot();
        var nicknameA = new Nickname("Alice");
        var nicknameB = new Nickname("Bobby");
        pot.PostBlind(nicknameA, new Chips(5));
        pot.PostBlind(nicknameB, new Chips(10));
        pot.PostBet(nicknameA, new Chips(20)); // Raise 25
        pot.PostBet(nicknameB, new Chips(15)); // Call 25

        // Act
        var (nickname, amount) = pot.CalculateRefund();

        // Assert
        Assert.Null(nickname);
        Assert.Equal(new Chips(0), amount);
    }

    [Fact]
    public void CalculateSidePots_NoAllIn_ShouldReturnSingle()
    {
        // Arrange
        var pot = CreatePot();
        var nicknameA = new Nickname("Alice");
        var nicknameB = new Nickname("Bobby");
        var nicknameC = new Nickname("Charlie");
        pot.PostAnte(new Chips(1));
        pot.PostAnte(new Chips(1));
        pot.PostAnte(new Chips(1));
        pot.PostBlind(nicknameA, new Chips(5));
        pot.PostBlind(nicknameB, new Chips(10));
        pot.PostBet(nicknameC, new Chips(25)); // Raise 25
        pot.PostBet(nicknameB, new Chips(15)); // Call 25
        pot.CommitBets();

        // Act
        var sidePots = pot.CalculateSidePots([nicknameB, nicknameC]).ToList();

        // Assert
        Assert.Single(sidePots);
        Assert.Equal([nicknameB, nicknameC], sidePots[0].Competitors);
        Assert.Equal(new Chips(58), sidePots[0].TotalAmount);
    }

    [Fact]
    public void CalculateSidePots_WithAllIn_ShouldReturnMultiple()
    {
        // Arrange
        var pot = CreatePot();
        var nicknameA = new Nickname("Alice"); // Stack is 801
        var nicknameB = new Nickname("Bobby"); // Stack is 901
        var nicknameC = new Nickname("Charlie"); // Stack is 1001
        pot.PostAnte(new Chips(1));
        pot.PostAnte(new Chips(1));
        pot.PostAnte(new Chips(1));
        pot.PostBlind(nicknameA, new Chips(5));
        pot.PostBlind(nicknameB, new Chips(10));
        pot.PostBet(nicknameC, new Chips(25)); // Raise 25
        pot.PostBet(nicknameA, new Chips(115)); // Raise 120
        pot.PostBet(nicknameB, new Chips(890)); // Raise 900 (all-in)
        pot.PostBet(nicknameC, new Chips(975)); // Raise 1000 (all-in)
        pot.PostBet(nicknameA, new Chips(680)); // Call 800 (all-in)
        pot.RefundBet(nicknameC, new Chips(100)); // Refund to 900
        pot.CommitBets();

        // Act
        var sidePots = pot.CalculateSidePots([nicknameA, nicknameB, nicknameC]).ToList();

        // Assert
        Assert.Equal(2, sidePots.Count);
        Assert.Equal([nicknameA, nicknameB, nicknameC], sidePots[0].Competitors);
        Assert.Equal(new Chips(2403), sidePots[0].TotalAmount);
        Assert.Equal([nicknameB, nicknameC], sidePots[1].Competitors);
        Assert.Equal(new Chips(200), sidePots[1].TotalAmount);
    }

    [Fact]
    public void CalculateSidePots_WithAnteOnly_ShouldReturnSingle()
    {
        // Arrange
        var pot = CreatePot();
        var nicknameA = new Nickname("Alice");
        var nicknameB = new Nickname("Bobby");
        var nicknameC = new Nickname("Charlie");
        pot.PostAnte(new Chips(1));
        pot.PostAnte(new Chips(1));
        pot.PostAnte(new Chips(1));

        // Act
        var sidePots = pot.CalculateSidePots([nicknameA, nicknameB, nicknameC]).ToList();

        // Assert
        Assert.Single(sidePots);
        Assert.Equal([nicknameA, nicknameB, nicknameC], sidePots[0].Competitors);
        Assert.Equal(new Chips(3), sidePots[0].TotalAmount);
    }

    [Fact]
    public void CalculateSidePots_WithEmptyPot_ShouldReturnNothing()
    {
        // Arrange
        var pot = CreatePot();
        var nicknameA = new Nickname("Alice");
        var nicknameB = new Nickname("Bobby");
        var nicknameC = new Nickname("Charlie");

        // Act
        var sidePots = pot.CalculateSidePots([nicknameA, nicknameB, nicknameC]).ToList();

        // Assert
        Assert.Empty(sidePots);
    }

    private Pot CreatePot()
    {
        return new Pot(minBet: new Chips(10));
    }
}
