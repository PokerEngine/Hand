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
        pot.PostBet(nicknameA, new Chips(20)); // Open raise to 25

        // Act
        pot.PostBet(nicknameB, new Chips(90)); // 3bet to 100

        // Assert
        Assert.Equal(new Chips(125), pot.TotalAmount);
        Assert.Equal(nicknameB, pot.LastPostedNickname);
        Assert.Equal(nicknameB, pot.LastRaisedNickname);
        Assert.Equal(new Chips(75), pot.LastRaisedStep);
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
        pot.PostBet(nicknameA, new Chips(20)); // Raise to 25
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

    private Pot CreatePot()
    {
        return new Pot(minBet: new Chips(10));
    }
}
