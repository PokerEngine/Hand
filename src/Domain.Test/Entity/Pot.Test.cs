using Domain.Entity;
using Domain.ValueObject;

namespace Domain.Test.Entity;

public class NoLimitPotTest
{
    [Fact]
    public void TestInitialization()
    {
        var pot = new NoLimitPot(
            smallBlind: new Chips(5),
            bigBlind: new Chips(10)
        );

        Assert.Equal(new Chips(5), pot.SmallBlind);
        Assert.Equal(new Chips(10), pot.BigBlind);
        Assert.Equal(new Chips(0), pot.GetTotalAmount());
    }

    [Fact]
    public void TestPostSmallBlind()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var pot = CreatePot();

        pot.PostSmallBlind(playerSb, new Chips(5));

        Assert.Equal(new Nickname("SmallBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(5), pot.GetTotalAmount());
        Assert.Equal(new Chips(5), pot.GetCurrentPostedAmount(playerSb));
        Assert.Equal(playerSb.Stake, new Chips(995));
        Assert.False(playerSb.IsFolded);
    }

    [Fact]
    public void TestPostSmallBlindAllIn()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1,
            stake: 2
        );
        var pot = CreatePot();

        pot.PostSmallBlind(playerSb, new Chips(2));

        Assert.Equal(new Nickname("SmallBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(2), pot.GetTotalAmount());
        Assert.Equal(new Chips(2), pot.GetCurrentPostedAmount(playerSb));
        Assert.Equal(playerSb.Stake, new Chips(0));
        Assert.False(playerSb.IsFolded);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(6)]
    public void TestPostSmallBlindWithWrongAmount(int amount)
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var pot = CreatePot();

        var exc = Assert.Throws<InvalidOperationException>(() => pot.PostSmallBlind(playerSb, new Chips(amount)));

        Assert.Equal("The player must post 5 chip(s)", exc.Message);
        Assert.Null(pot.LastDecisionNickname);
        Assert.Equal(new Chips(0), pot.GetTotalAmount());
        Assert.Equal(new Chips(0), pot.GetCurrentPostedAmount(playerSb));
        Assert.Equal(playerSb.Stake, new Chips(1000));
        Assert.False(playerSb.IsFolded);
    }

    [Fact]
    public void TestPostBigBlind()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        pot.PostSmallBlind(playerSb, new Chips(5));

        pot.PostBigBlind(playerBb, new Chips(10));

        Assert.Equal(new Nickname("BigBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(15), pot.GetTotalAmount());
        Assert.Equal(new Chips(10), pot.GetCurrentPostedAmount(playerBb));
        Assert.Equal(playerBb.Stake, new Chips(990));
        Assert.False(playerBb.IsFolded);
    }

    [Fact]
    public void TestPostBigBlindAllIn()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2,
            stake: 2
        );
        var pot = CreatePot();
        pot.PostSmallBlind(playerSb, new Chips(5));

        pot.PostBigBlind(playerBb, new Chips(2));

        Assert.Equal(new Nickname("BigBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(7), pot.GetTotalAmount());
        Assert.Equal(new Chips(2), pot.GetCurrentPostedAmount(playerBb));
        Assert.Equal(playerBb.Stake, new Chips(0));
        Assert.False(playerBb.IsFolded);
    }

    [Fact]
    public void TestPostBigBlindWithNoSmallBlind()
    {
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();

        pot.PostBigBlind(playerBb, new Chips(10));

        Assert.Equal(new Nickname("BigBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(10), pot.GetTotalAmount());
        Assert.Equal(new Chips(10), pot.GetCurrentPostedAmount(playerBb));
        Assert.Equal(playerBb.Stake, new Chips(990));
        Assert.False(playerBb.IsFolded);
    }

    [Theory]
    [InlineData(9)]
    [InlineData(11)]
    public void TestPostBigBlindWithWrongAmount(int amount)
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        pot.PostSmallBlind(playerSb, new Chips(5));

        var exc = Assert.Throws<InvalidOperationException>(() => pot.PostBigBlind(playerBb, new Chips(amount)));

        Assert.Equal("The player must post 10 chip(s)", exc.Message);
        Assert.Equal(new Nickname("SmallBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(5), pot.GetTotalAmount());
        Assert.Equal(new Chips(0), pot.GetCurrentPostedAmount(playerBb));
        Assert.Equal(playerBb.Stake, new Chips(1000));
        Assert.False(playerBb.IsFolded);
    }

    [Fact]
    public void TestFold()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));

        Assert.True(pot.DecisionIsAvailable(playerSb));
        Assert.True(pot.FoldIsAvailable(playerSb));
        Assert.False(pot.CheckIsAvailable(playerSb));
        Assert.True(pot.CallIsAvailable(playerSb));
        Assert.Equal(new Chips(10), pot.GetCallToAmount(playerSb));
        Assert.True(pot.RaiseIsAvailable(playerSb));
        Assert.Equal(new Chips(20), pot.GetMinRaiseToAmount(playerSb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerSb));

        pot.CommitDecision(playerSb, new Decision(DecisionType.Fold, new Chips(0)));

        Assert.False(pot.DecisionIsAvailable(playerBb));

        Assert.Equal(new Nickname("SmallBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(15), pot.GetTotalAmount());
        Assert.Equal(new Chips(0), pot.GetCurrentPostedAmount(playerSb));
        Assert.Equal(playerSb.Stake, new Chips(995));
        Assert.True(playerSb.IsFolded);
    }

    [Fact]
    public void TestFoldWhenPostedMoreAmount()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerSb, new Decision(DecisionType.Fold, new Chips(0)));

        Assert.False(pot.DecisionIsAvailable(playerBb));

        var exc = Assert.Throws<InvalidOperationException>(() => pot.CommitDecision(playerBb, new Decision(DecisionType.Fold, new Chips(0))));

        Assert.Equal("The player has posted the most amount into the pot, he cannot fold", exc.Message);
        Assert.Equal(new Nickname("SmallBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(15), pot.GetTotalAmount());
        Assert.Equal(new Chips(10), pot.GetCurrentPostedAmount(playerBb));
        Assert.Equal(playerBb.Stake, new Chips(990));
        Assert.False(playerBb.IsFolded);
    }

    [Fact]
    public void TestFoldWhenPostedSameAmount()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerSb, new Decision(DecisionType.CallTo, new Chips(10)));

        Assert.True(pot.DecisionIsAvailable(playerBb));
        Assert.False(pot.FoldIsAvailable(playerBb));
        Assert.True(pot.CheckIsAvailable(playerBb));
        Assert.False(pot.CallIsAvailable(playerBb));
        Assert.True(pot.RaiseIsAvailable(playerBb));
        Assert.Equal(new Chips(20), pot.GetMinRaiseToAmount(playerSb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerSb));

        var exc = Assert.Throws<InvalidOperationException>(() => pot.CommitDecision(playerBb, new Decision(DecisionType.Fold, new Chips(0))));

        Assert.Equal("The player has posted the same amount into the pot, he cannot fold", exc.Message);
        Assert.Equal(new Nickname("SmallBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(20), pot.GetTotalAmount());
        Assert.Equal(new Chips(10), pot.GetCurrentPostedAmount(playerBb));
        Assert.Equal(playerBb.Stake, new Chips(990));
        Assert.False(playerBb.IsFolded);
    }

    [Fact]
    public void TestCheck()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerSb, new Decision(DecisionType.CallTo, new Chips(10)));

        Assert.True(pot.DecisionIsAvailable(playerBb));
        Assert.False(pot.FoldIsAvailable(playerBb));
        Assert.True(pot.CheckIsAvailable(playerBb));
        Assert.False(pot.CallIsAvailable(playerBb));
        Assert.True(pot.RaiseIsAvailable(playerBb));
        Assert.Equal(new Chips(20), pot.GetMinRaiseToAmount(playerBb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerBb));

        pot.CommitDecision(playerBb, new Decision(DecisionType.Check, new Chips(0)));

        Assert.False(pot.DecisionIsAvailable(playerSb));

        Assert.Equal(new Nickname("BigBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(20), pot.GetTotalAmount());
        Assert.Equal(new Chips(10), pot.GetCurrentPostedAmount(playerBb));
        Assert.Equal(playerBb.Stake, new Chips(990));
        Assert.False(playerBb.IsFolded);
    }

    [Fact]
    public void TestCheckWhenPostedMoreAmount()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerSb, new Decision(DecisionType.Fold, new Chips(0)));

        Assert.False(pot.DecisionIsAvailable(playerBb));

        var exc = Assert.Throws<InvalidOperationException>(() => pot.CommitDecision(playerBb, new Decision(DecisionType.Check, new Chips(0))));

        Assert.Equal("The player has posted the most amount into the pot, he cannot check", exc.Message);
        Assert.Equal(new Nickname("SmallBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(15), pot.GetTotalAmount());
        Assert.Equal(new Chips(10), pot.GetCurrentPostedAmount(playerBb));
        Assert.Equal(playerBb.Stake, new Chips(990));
        Assert.False(playerBb.IsFolded);
    }

    [Fact]
    public void TestCheckWhenPostedLessAmount()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));

        Assert.True(pot.DecisionIsAvailable(playerSb));
        Assert.True(pot.FoldIsAvailable(playerSb));
        Assert.False(pot.CheckIsAvailable(playerSb));
        Assert.True(pot.CallIsAvailable(playerSb));
        Assert.Equal(new Chips(10), pot.GetCallToAmount(playerSb));
        Assert.True(pot.RaiseIsAvailable(playerSb));
        Assert.Equal(new Chips(20), pot.GetMinRaiseToAmount(playerSb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerSb));

        var exc = Assert.Throws<InvalidOperationException>(() => pot.CommitDecision(playerSb, new Decision(DecisionType.Check, new Chips(0))));

        Assert.Equal("The player has posted less amount into the pot, he cannot check", exc.Message);
        Assert.Equal(new Nickname("BigBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(15), pot.GetTotalAmount());
        Assert.Equal(new Chips(5), pot.GetCurrentPostedAmount(playerSb));
        Assert.Equal(playerSb.Stake, new Chips(995));
        Assert.False(playerSb.IsFolded);
    }

    [Fact]
    public void TestCheckWhenPerformedDecision()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerSb, new Decision(DecisionType.CallTo, new Chips(10)));

        Assert.False(pot.DecisionIsAvailable(playerSb));

        var exc = Assert.Throws<InvalidOperationException>(() => pot.CommitDecision(playerSb, new Decision(DecisionType.Check, new Chips(0))));

        Assert.Equal("The player has already performed an action, he cannot check", exc.Message);
        Assert.Equal(new Nickname("SmallBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(20), pot.GetTotalAmount());
        Assert.Equal(new Chips(10), pot.GetCurrentPostedAmount(playerSb));
        Assert.Equal(playerSb.Stake, new Chips(990));
        Assert.False(playerSb.IsFolded);
    }

    [Fact]
    public void TestCall()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));

        Assert.True(pot.DecisionIsAvailable(playerSb));
        Assert.True(pot.FoldIsAvailable(playerSb));
        Assert.False(pot.CheckIsAvailable(playerSb));
        Assert.True(pot.CallIsAvailable(playerSb));
        Assert.Equal(new Chips(10), pot.GetCallToAmount(playerSb));
        Assert.True(pot.RaiseIsAvailable(playerSb));
        Assert.Equal(new Chips(20), pot.GetMinRaiseToAmount(playerSb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerSb));

        pot.CommitDecision(playerSb, new Decision(DecisionType.CallTo, new Chips(10)));

        Assert.True(pot.DecisionIsAvailable(playerBb));
        Assert.False(pot.FoldIsAvailable(playerBb));
        Assert.True(pot.CheckIsAvailable(playerBb));
        Assert.False(pot.CallIsAvailable(playerBb));
        Assert.True(pot.RaiseIsAvailable(playerBb));
        Assert.Equal(new Chips(20), pot.GetMinRaiseToAmount(playerBb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerBb));

        Assert.Equal(new Nickname("SmallBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(20), pot.GetTotalAmount());
        Assert.Equal(new Chips(10), pot.GetCurrentPostedAmount(playerSb));
        Assert.Equal(playerSb.Stake, new Chips(990));
        Assert.False(playerSb.IsFolded);
    }

    [Fact]
    public void TestCallAllIn()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2,
            stake: 19
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerSb, new Decision(DecisionType.RaiseTo, new Chips(20)));

        Assert.True(pot.DecisionIsAvailable(playerBb));
        Assert.True(pot.FoldIsAvailable(playerBb));
        Assert.False(pot.CheckIsAvailable(playerBb));
        Assert.True(pot.CallIsAvailable(playerBb));
        Assert.Equal(new Chips(19), pot.GetCallToAmount(playerBb));
        Assert.False(pot.RaiseIsAvailable(playerBb));

        pot.CommitDecision(playerBb, new Decision(DecisionType.CallTo, new Chips(19)));

        Assert.False(pot.DecisionIsAvailable(playerSb));

        Assert.Equal(new Nickname("BigBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(39), pot.GetTotalAmount());
        Assert.Equal(new Chips(19), pot.GetCurrentPostedAmount(playerBb));
        Assert.Equal(playerBb.Stake, new Chips(0));
        Assert.False(playerBb.IsFolded);
    }

    [Fact]
    public void TestCallWhenPostedMoreAmount()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerSb, new Decision(DecisionType.Fold, new Chips(0)));

        Assert.False(pot.DecisionIsAvailable(playerBb));

        var exc = Assert.Throws<InvalidOperationException>(() => pot.CommitDecision(playerBb, new Decision(DecisionType.CallTo, new Chips(10))));

        Assert.Equal("The player has posted the most amount into the pot, he cannot call", exc.Message);
        Assert.Equal(new Nickname("SmallBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(15), pot.GetTotalAmount());
        Assert.Equal(new Chips(10), pot.GetCurrentPostedAmount(playerBb));
        Assert.Equal(playerBb.Stake, new Chips(990));
        Assert.False(playerBb.IsFolded);
    }

    [Fact]
    public void TestCallWhenPostedSameAmount()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerSb, new Decision(DecisionType.CallTo, new Chips(10)));

        Assert.True(pot.DecisionIsAvailable(playerBb));
        Assert.False(pot.FoldIsAvailable(playerBb));
        Assert.True(pot.CheckIsAvailable(playerBb));
        Assert.False(pot.CallIsAvailable(playerBb));
        Assert.True(pot.RaiseIsAvailable(playerBb));
        Assert.Equal(new Chips(20), pot.GetMinRaiseToAmount(playerBb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerBb));

        var exc = Assert.Throws<InvalidOperationException>(() => pot.CommitDecision(playerBb, new Decision(DecisionType.CallTo, new Chips(10))));

        Assert.Equal("The player has posted the same amount into the pot, he cannot call", exc.Message);
        Assert.Equal(new Nickname("SmallBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(20), pot.GetTotalAmount());
        Assert.Equal(new Chips(10), pot.GetCurrentPostedAmount(playerBb));
        Assert.Equal(playerBb.Stake, new Chips(990));
        Assert.False(playerBb.IsFolded);
    }

    [Theory]
    [InlineData(9)]
    [InlineData(11)]
    public void TestCallToWrongAmount(int amount)
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));

        Assert.True(pot.DecisionIsAvailable(playerSb));
        Assert.True(pot.FoldIsAvailable(playerSb));
        Assert.False(pot.CheckIsAvailable(playerSb));
        Assert.True(pot.CallIsAvailable(playerSb));
        Assert.Equal(new Chips(10), pot.GetCallToAmount(playerSb));
        Assert.True(pot.RaiseIsAvailable(playerSb));
        Assert.Equal(new Chips(20), pot.GetMinRaiseToAmount(playerSb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerSb));

        var exc = Assert.Throws<InvalidOperationException>(() => pot.CommitDecision(playerSb, new Decision(DecisionType.CallTo, new Chips(amount))));

        Assert.Equal("The player must call to 10 chip(s)", exc.Message);
        Assert.Equal(new Nickname("BigBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(15), pot.GetTotalAmount());
        Assert.Equal(new Chips(5), pot.GetCurrentPostedAmount(playerSb));
        Assert.Equal(playerSb.Stake, new Chips(995));
        Assert.False(playerSb.IsFolded);
    }

    [Fact]
    public void TestCallAllInToLessAmount()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1,
            stake: 9
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));

        Assert.True(pot.DecisionIsAvailable(playerSb));
        Assert.True(pot.FoldIsAvailable(playerSb));
        Assert.False(pot.CheckIsAvailable(playerSb));
        Assert.True(pot.CallIsAvailable(playerSb));
        Assert.Equal(new Chips(9), pot.GetCallToAmount(playerSb));
        Assert.False(pot.RaiseIsAvailable(playerSb));

        var exc = Assert.Throws<InvalidOperationException>(() => pot.CommitDecision(playerSb, new Decision(DecisionType.CallTo, new Chips(8))));

        Assert.Equal("The player must call to 9 chip(s)", exc.Message);
        Assert.Equal(new Nickname("BigBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(15), pot.GetTotalAmount());
        Assert.Equal(new Chips(5), pot.GetCurrentPostedAmount(playerSb));
        Assert.Equal(playerSb.Stake, new Chips(4));
        Assert.False(playerSb.IsFolded);
    }

    [Fact]
    public void TestRaise()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));

        Assert.True(pot.DecisionIsAvailable(playerSb));
        Assert.True(pot.FoldIsAvailable(playerSb));
        Assert.False(pot.CheckIsAvailable(playerSb));
        Assert.True(pot.CallIsAvailable(playerSb));
        Assert.Equal(new Chips(10), pot.GetCallToAmount(playerSb));
        Assert.True(pot.RaiseIsAvailable(playerSb));
        Assert.Equal(new Chips(20), pot.GetMinRaiseToAmount(playerSb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerSb));

        pot.CommitDecision(playerSb, new Decision(DecisionType.RaiseTo, new Chips(20)));

        Assert.True(pot.DecisionIsAvailable(playerBb));
        Assert.True(pot.FoldIsAvailable(playerBb));
        Assert.False(pot.CheckIsAvailable(playerBb));
        Assert.True(pot.CallIsAvailable(playerBb));
        Assert.Equal(new Chips(20), pot.GetCallToAmount(playerBb));
        Assert.True(pot.RaiseIsAvailable(playerBb));
        Assert.Equal(new Chips(30), pot.GetMinRaiseToAmount(playerBb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerBb));

        Assert.Equal(new Nickname("SmallBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(30), pot.GetTotalAmount());
        Assert.Equal(new Chips(20), pot.GetCurrentPostedAmount(playerSb));
        Assert.Equal(playerSb.Stake, new Chips(980));
        Assert.False(playerSb.IsFolded);
    }

    [Fact]
    public void TestRaiseAllIn()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1,
            stake: 19
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));

        Assert.True(pot.DecisionIsAvailable(playerSb));
        Assert.True(pot.FoldIsAvailable(playerSb));
        Assert.False(pot.CheckIsAvailable(playerSb));
        Assert.True(pot.CallIsAvailable(playerSb));
        Assert.Equal(new Chips(10), pot.GetCallToAmount(playerSb));
        Assert.True(pot.RaiseIsAvailable(playerSb));
        Assert.Equal(new Chips(19), pot.GetMinRaiseToAmount(playerSb));
        Assert.Equal(new Chips(19), pot.GetMaxRaiseToAmount(playerSb));

        pot.CommitDecision(playerSb, new Decision(DecisionType.RaiseTo, new Chips(19)));

        Assert.True(pot.DecisionIsAvailable(playerBb));
        Assert.True(pot.FoldIsAvailable(playerBb));
        Assert.False(pot.CheckIsAvailable(playerBb));
        Assert.True(pot.CallIsAvailable(playerBb));
        Assert.Equal(new Chips(19), pot.GetCallToAmount(playerBb));
        Assert.True(pot.RaiseIsAvailable(playerBb));
        Assert.Equal(new Chips(29), pot.GetMinRaiseToAmount(playerBb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerBb));

        Assert.Equal(new Nickname("SmallBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(29), pot.GetTotalAmount());
        Assert.Equal(new Chips(19), pot.GetCurrentPostedAmount(playerSb));
        Assert.Equal(playerSb.Stake, new Chips(0));
        Assert.False(playerSb.IsFolded);
    }

    [Fact]
    public void TestRaiseWhenPostedMoreAmount()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerSb, new Decision(DecisionType.Fold, new Chips(0)));

        Assert.False(pot.DecisionIsAvailable(playerBb));

        var exc = Assert.Throws<InvalidOperationException>(() => pot.CommitDecision(playerBb, new Decision(DecisionType.RaiseTo, new Chips(20))));

        Assert.Equal("The player has posted the most amount into the pot, he cannot raise", exc.Message);
        Assert.Equal(new Nickname("SmallBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(15), pot.GetTotalAmount());
        Assert.Equal(new Chips(10), pot.GetCurrentPostedAmount(playerBb));
        Assert.Equal(playerBb.Stake, new Chips(990));
        Assert.False(playerBb.IsFolded);
    }

    [Fact]
    public void TestRaiseWhenPostedSameAmount()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerSb, new Decision(DecisionType.CallTo, new Chips(10)));

        Assert.True(pot.DecisionIsAvailable(playerBb));
        Assert.False(pot.FoldIsAvailable(playerBb));
        Assert.True(pot.CheckIsAvailable(playerBb));
        Assert.False(pot.CallIsAvailable(playerBb));
        Assert.True(pot.RaiseIsAvailable(playerBb));
        Assert.Equal(new Chips(20), pot.GetMinRaiseToAmount(playerBb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerBb));

        pot.CommitDecision(playerBb, new Decision(DecisionType.RaiseTo, new Chips(20)));

        Assert.True(pot.DecisionIsAvailable(playerSb));
        Assert.True(pot.FoldIsAvailable(playerSb));
        Assert.False(pot.CheckIsAvailable(playerSb));
        Assert.True(pot.CallIsAvailable(playerSb));
        Assert.Equal(new Chips(20), pot.GetCallToAmount(playerSb));
        Assert.True(pot.RaiseIsAvailable(playerSb));
        Assert.Equal(new Chips(30), pot.GetMinRaiseToAmount(playerSb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerSb));

        Assert.Equal(new Nickname("BigBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(30), pot.GetTotalAmount());
        Assert.Equal(new Chips(20), pot.GetCurrentPostedAmount(playerBb));
        Assert.Equal(playerBb.Stake, new Chips(980));
        Assert.False(playerBb.IsFolded);
    }

    [Fact]
    public void TestRaiseToLessThanMinimumAmount()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));

        Assert.True(pot.DecisionIsAvailable(playerSb));
        Assert.True(pot.FoldIsAvailable(playerSb));
        Assert.False(pot.CheckIsAvailable(playerSb));
        Assert.True(pot.CallIsAvailable(playerSb));
        Assert.Equal(new Chips(10), pot.GetCallToAmount(playerSb));
        Assert.True(pot.RaiseIsAvailable(playerSb));
        Assert.Equal(new Chips(20), pot.GetMinRaiseToAmount(playerSb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerSb));

        var exc = Assert.Throws<InvalidOperationException>(() => pot.CommitDecision(playerSb, new Decision(DecisionType.RaiseTo, new Chips(19))));

        Assert.Equal("The player must raise to minimum 20 chip(s)", exc.Message);
        Assert.Equal(new Nickname("BigBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(15), pot.GetTotalAmount());
        Assert.Equal(new Chips(5), pot.GetCurrentPostedAmount(playerSb));
        Assert.Equal(playerSb.Stake, new Chips(995));
        Assert.False(playerSb.IsFolded);
    }

    [Fact]
    public void TestRaiseToMoreThanMaximumAmount()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));

        Assert.True(pot.DecisionIsAvailable(playerSb));
        Assert.True(pot.FoldIsAvailable(playerSb));
        Assert.False(pot.CheckIsAvailable(playerSb));
        Assert.True(pot.CallIsAvailable(playerSb));
        Assert.Equal(new Chips(10), pot.GetCallToAmount(playerSb));
        Assert.True(pot.RaiseIsAvailable(playerSb));
        Assert.Equal(new Chips(20), pot.GetMinRaiseToAmount(playerSb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerSb));

        var exc = Assert.Throws<InvalidOperationException>(() => pot.CommitDecision(playerSb, new Decision(DecisionType.RaiseTo, new Chips(1001))));

        Assert.Equal("The player must raise to maximum 1000 chip(s)", exc.Message);
        Assert.Equal(new Nickname("BigBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(15), pot.GetTotalAmount());
        Assert.Equal(new Chips(5), pot.GetCurrentPostedAmount(playerSb));
        Assert.Equal(playerSb.Stake, new Chips(995));
        Assert.False(playerSb.IsFolded);
    }

    [Fact]
    public void TestRaiseAllInToLessThanCallAmount()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2,
            stake: 19
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerSb, new Decision(DecisionType.RaiseTo, new Chips(20)));

        Assert.True(pot.DecisionIsAvailable(playerBb));
        Assert.True(pot.FoldIsAvailable(playerBb));
        Assert.False(pot.CheckIsAvailable(playerBb));
        Assert.True(pot.CallIsAvailable(playerBb));
        Assert.Equal(new Chips(19), pot.GetCallToAmount(playerBb));
        Assert.False(pot.RaiseIsAvailable(playerBb));

        var exc = Assert.Throws<InvalidOperationException>(() => pot.CommitDecision(playerBb, new Decision(DecisionType.RaiseTo, new Chips(19))));

        Assert.Equal("The player must call to 19 chip(s)", exc.Message);
        Assert.Equal(new Nickname("SmallBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(30), pot.GetTotalAmount());
        Assert.Equal(new Chips(10), pot.GetCurrentPostedAmount(playerBb));
        Assert.Equal(playerBb.Stake, new Chips(9));
        Assert.False(playerBb.IsFolded);
    }

    [Fact]
    public void TestRaiseAllInToLessThanMinimumAmount()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1,
            stake: 19
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));

        Assert.True(pot.DecisionIsAvailable(playerSb));
        Assert.True(pot.FoldIsAvailable(playerSb));
        Assert.False(pot.CheckIsAvailable(playerSb));
        Assert.True(pot.CallIsAvailable(playerSb));
        Assert.Equal(new Chips(10), pot.GetCallToAmount(playerSb));
        Assert.True(pot.RaiseIsAvailable(playerSb));
        Assert.Equal(new Chips(19), pot.GetMinRaiseToAmount(playerSb));
        Assert.Equal(new Chips(19), pot.GetMaxRaiseToAmount(playerSb));

        var exc = Assert.Throws<InvalidOperationException>(() => pot.CommitDecision(playerSb, new Decision(DecisionType.RaiseTo, new Chips(18))));

        Assert.Equal("The player must raise to minimum 19 chip(s)", exc.Message);
        Assert.Equal(new Nickname("BigBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(15), pot.GetTotalAmount());
        Assert.Equal(new Chips(5), pot.GetCurrentPostedAmount(playerSb));
        Assert.Equal(playerSb.Stake, new Chips(14));
        Assert.False(playerSb.IsFolded);
    }

    [Fact]
    public void TestRaiseWhenThereWasRaiseAllIn()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1,
            stake: 40
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 6
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(25)));
        pot.CommitDecision(playerSb, new Decision(DecisionType.RaiseTo, new Chips(40)));  // Is considered a raise
        pot.CommitDecision(playerBb, new Decision(DecisionType.CallTo, new Chips(40)));

        Assert.True(pot.DecisionIsAvailable(playerBu));
        Assert.True(pot.FoldIsAvailable(playerBu));
        Assert.False(pot.CheckIsAvailable(playerBu));
        Assert.True(pot.CallIsAvailable(playerBu));
        Assert.Equal(new Chips(40), pot.GetCallToAmount(playerBu));
        Assert.True(pot.RaiseIsAvailable(playerBu));
        Assert.Equal(new Chips(55), pot.GetMinRaiseToAmount(playerBu));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerBu));

        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(55)));

        Assert.True(pot.DecisionIsAvailable(playerBb));
        Assert.True(pot.FoldIsAvailable(playerBb));
        Assert.False(pot.CheckIsAvailable(playerBb));
        Assert.True(pot.CallIsAvailable(playerBb));
        Assert.Equal(new Chips(55), pot.GetCallToAmount(playerBb));
        Assert.True(pot.RaiseIsAvailable(playerBb));
        Assert.Equal(new Chips(70), pot.GetMinRaiseToAmount(playerBb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerBb));

        Assert.Equal(new Nickname("Button"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(135), pot.GetTotalAmount());
        Assert.Equal(new Chips(40), pot.GetCurrentPostedAmount(playerSb));
        Assert.Equal(new Chips(40), pot.GetCurrentPostedAmount(playerBb));
        Assert.Equal(new Chips(55), pot.GetCurrentPostedAmount(playerBu));
        Assert.Equal(playerSb.Stake, new Chips(0));
        Assert.False(playerSb.IsFolded);
        Assert.Equal(playerBb.Stake, new Chips(960));
        Assert.False(playerBb.IsFolded);
        Assert.Equal(playerBu.Stake, new Chips(945));
        Assert.False(playerBu.IsFolded);
    }

    [Fact]
    public void TestRaiseWhenThereWasNoRaiseAllIn()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1,
            stake: 39
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 6
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(25)));
        pot.CommitDecision(playerSb, new Decision(DecisionType.RaiseTo, new Chips(39)));  // Is not considered a raise because the current step is 15
        pot.CommitDecision(playerBb, new Decision(DecisionType.CallTo, new Chips(39)));

        Assert.True(pot.DecisionIsAvailable(playerBu));
        Assert.True(pot.FoldIsAvailable(playerBu));
        Assert.False(pot.CheckIsAvailable(playerBu));
        Assert.True(pot.CallIsAvailable(playerBu));
        Assert.Equal(new Chips(39), pot.GetCallToAmount(playerBu));
        Assert.False(pot.RaiseIsAvailable(playerBu));

        var exc = Assert.Throws<InvalidOperationException>(() => pot.CommitDecision(playerBb, new Decision(DecisionType.RaiseTo, new Chips(55))));

        Assert.Equal("There was no raise since the player's last action, he cannot raise", exc.Message);
        Assert.Equal(new Nickname("BigBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(103), pot.GetTotalAmount());
        Assert.Equal(new Chips(39), pot.GetCurrentPostedAmount(playerSb));
        Assert.Equal(new Chips(39), pot.GetCurrentPostedAmount(playerBb));
        Assert.Equal(new Chips(25), pot.GetCurrentPostedAmount(playerBu));
        Assert.Equal(playerSb.Stake, new Chips(0));
        Assert.False(playerSb.IsFolded);
        Assert.Equal(playerBb.Stake, new Chips(961));
        Assert.False(playerBb.IsFolded);
        Assert.Equal(playerBu.Stake, new Chips(975));
        Assert.False(playerBu.IsFolded);
    }

    [Fact]
    public void TestRaiseWhenThereWasNoRaiseAllInAndThenRaise()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1,
            stake: 39
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 6
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(25)));
        pot.CommitDecision(playerSb, new Decision(DecisionType.RaiseTo, new Chips(39)));  // Is not considered a raise because the current step is 15
        pot.CommitDecision(playerBb, new Decision(DecisionType.RaiseTo, new Chips(54)));  // Is considered a raise

        Assert.True(pot.DecisionIsAvailable(playerBu));
        Assert.True(pot.FoldIsAvailable(playerBu));
        Assert.False(pot.CheckIsAvailable(playerBu));
        Assert.True(pot.CallIsAvailable(playerBu));
        Assert.Equal(new Chips(54), pot.GetCallToAmount(playerBu));
        Assert.True(pot.RaiseIsAvailable(playerBu));
        Assert.Equal(new Chips(69), pot.GetMinRaiseToAmount(playerBu));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerBu));

        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(69)));

        Assert.True(pot.DecisionIsAvailable(playerBb));
        Assert.True(pot.FoldIsAvailable(playerBb));
        Assert.False(pot.CheckIsAvailable(playerBb));
        Assert.True(pot.CallIsAvailable(playerBb));
        Assert.Equal(new Chips(69), pot.GetCallToAmount(playerBb));
        Assert.True(pot.RaiseIsAvailable(playerBb));
        Assert.Equal(new Chips(84), pot.GetMinRaiseToAmount(playerBb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerBb));

        Assert.Equal(new Nickname("Button"), pot.LastDecisionNickname);
        Assert.Equal(new Chips(162), pot.GetTotalAmount());
        Assert.Equal(new Chips(39), pot.GetCurrentPostedAmount(playerSb));
        Assert.Equal(new Chips(54), pot.GetCurrentPostedAmount(playerBb));
        Assert.Equal(new Chips(69), pot.GetCurrentPostedAmount(playerBu));
        Assert.Equal(playerSb.Stake, new Chips(0));
        Assert.False(playerSb.IsFolded);
        Assert.Equal(playerBb.Stake, new Chips(946));
        Assert.False(playerBb.IsFolded);
        Assert.Equal(playerBu.Stake, new Chips(931));
        Assert.False(playerBu.IsFolded);
    }

    [Fact]
    public void TestFinishStage()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 6
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(25)));
        pot.CommitDecision(playerSb, new Decision(DecisionType.RaiseTo, new Chips(120)));
        pot.CommitDecision(playerBb, new Decision(DecisionType.Fold, new Chips(0)));
        pot.CommitDecision(playerBu, new Decision(DecisionType.CallTo, new Chips(120)));

        Assert.False(pot.DecisionIsAvailable(playerSb));
        Assert.Equal(new Chips(120), pot.GetCurrentPostedAmount(playerSb));
        Assert.Equal(new Chips(0), pot.GetPreviousPostedAmount(playerSb));
        Assert.False(pot.DecisionIsAvailable(playerBu));
        Assert.Equal(new Chips(120), pot.GetCurrentPostedAmount(playerBu));
        Assert.Equal(new Chips(0), pot.GetPreviousPostedAmount(playerBu));

        pot.FinishStage();

        Assert.True(pot.DecisionIsAvailable(playerSb));
        Assert.False(pot.FoldIsAvailable(playerSb));
        Assert.True(pot.CheckIsAvailable(playerSb));
        Assert.False(pot.CallIsAvailable(playerSb));
        Assert.True(pot.RaiseIsAvailable(playerSb));
        Assert.Equal(new Chips(0), pot.GetCurrentPostedAmount(playerSb));
        Assert.Equal(new Chips(120), pot.GetPreviousPostedAmount(playerSb));
        Assert.Equal(new Chips(10), pot.GetMinRaiseToAmount(playerSb));
        Assert.Equal(new Chips(880), pot.GetMaxRaiseToAmount(playerSb));

        Assert.Equal(new Chips(0), pot.GetCurrentPostedAmount(playerBu));
        Assert.Equal(new Chips(120), pot.GetPreviousPostedAmount(playerBu));

        Assert.Equal(new Chips(250), pot.GetTotalAmount());
        Assert.Null(pot.LastDecisionNickname);
    }

    [Fact]
    public void TestGetSidePots()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1,
            stake: 100
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2,
            stake: 200
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 6,
            stake: 600
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(300)));
        pot.CommitDecision(playerSb, new Decision(DecisionType.CallTo, new Chips(100)));
        pot.CommitDecision(playerBb, new Decision(DecisionType.CallTo, new Chips(200)));

        var sidePots = pot.GetSidePots([playerSb, playerBb, playerBu]);

        Assert.Equal(3, sidePots.Count);

        var expectedSidePot1 = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerSb.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerBb.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerBu.Nickname, new Chips(100)),
        ]);
        var expectedSidePot2 = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerBb.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerBu.Nickname, new Chips(100)),
        ]);
        var expectedSidePot3 = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerBu.Nickname, new Chips(100)),
        ]);

        Assert.Equal(expectedSidePot1, sidePots[0]);
        Assert.Equal(expectedSidePot2, sidePots[1]);
        Assert.Equal(expectedSidePot3, sidePots[2]);
    }

    [Fact]
    public void TestRefundWhenAllOpponentsFolded()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 6
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(25)));
        pot.CommitDecision(playerSb, new Decision(DecisionType.RaiseTo, new Chips(120)));
        pot.CommitDecision(playerBb, new Decision(DecisionType.Fold, new Chips(0)));
        pot.CommitDecision(playerBu, new Decision(DecisionType.Fold, new Chips(0)));
        pot.FinishStage();

        Assert.Equal(new Chips(95), pot.GetRefundAmount(playerSb));
        Assert.Equal(new Chips(120), pot.GetPreviousPostedAmount(playerSb));
        Assert.Equal(new Chips(155), pot.GetTotalAmount());

        pot.CommitRefund(playerSb, new Chips(95));

        Assert.Equal(new Chips(0), pot.GetRefundAmount(playerSb));
        Assert.Equal(new Chips(25), pot.GetPreviousPostedAmount(playerSb));
        Assert.Equal(new Chips(60), pot.GetTotalAmount());

        Assert.Equal(new Chips(975), playerSb.Stake);
    }

    [Fact]
    public void TestRefundWhenThereWasAllIn()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1,
            stake: 500
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2,
            stake: 250
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 6
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(25)));
        pot.CommitDecision(playerSb, new Decision(DecisionType.RaiseTo, new Chips(120)));
        pot.CommitDecision(playerBb, new Decision(DecisionType.RaiseTo, new Chips(250)));
        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(1000)));
        pot.CommitDecision(playerSb, new Decision(DecisionType.CallTo, new Chips(500)));
        pot.FinishStage();

        Assert.Equal(new Chips(0), pot.GetRefundAmount(playerSb));
        Assert.Equal(new Chips(500), pot.GetPreviousPostedAmount(playerSb));
        Assert.Equal(new Chips(0), pot.GetRefundAmount(playerBb));
        Assert.Equal(new Chips(250), pot.GetPreviousPostedAmount(playerBb));
        Assert.Equal(new Chips(500), pot.GetRefundAmount(playerBu));
        Assert.Equal(new Chips(1000), pot.GetPreviousPostedAmount(playerBu));
        Assert.Equal(new Chips(1750), pot.GetTotalAmount());

        pot.CommitRefund(playerBu, new Chips(500));

        Assert.Equal(new Chips(0), pot.GetRefundAmount(playerBu));
        Assert.Equal(new Chips(500), pot.GetPreviousPostedAmount(playerBu));
        Assert.Equal(new Chips(1250), pot.GetTotalAmount());
    }

    [Fact]
    public void TestRefundWhenPostedLessAmount()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1,
            stake: 500
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2,
            stake: 250
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 6
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(25)));
        pot.CommitDecision(playerSb, new Decision(DecisionType.RaiseTo, new Chips(120)));
        pot.CommitDecision(playerBb, new Decision(DecisionType.RaiseTo, new Chips(250)));
        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(1000)));
        pot.CommitDecision(playerSb, new Decision(DecisionType.CallTo, new Chips(500)));
        pot.FinishStage();

        Assert.Equal(new Chips(0), pot.GetRefundAmount(playerSb));
        Assert.Equal(new Chips(500), pot.GetPreviousPostedAmount(playerSb));
        Assert.Equal(new Chips(0), pot.GetRefundAmount(playerBb));
        Assert.Equal(new Chips(250), pot.GetPreviousPostedAmount(playerBb));
        Assert.Equal(new Chips(500), pot.GetRefundAmount(playerBu));
        Assert.Equal(new Chips(1000), pot.GetPreviousPostedAmount(playerBu));
        Assert.Equal(new Chips(1750), pot.GetTotalAmount());

        var exc = Assert.Throws<InvalidOperationException>(() => pot.CommitRefund(playerSb, new Chips(0)));

        Assert.Equal("The player cannot refund", exc.Message);
        Assert.Equal(new Chips(0), pot.GetRefundAmount(playerSb));
        Assert.Equal(new Chips(500), pot.GetPreviousPostedAmount(playerSb));
        Assert.Equal(new Chips(0), pot.GetRefundAmount(playerBb));
        Assert.Equal(new Chips(250), pot.GetPreviousPostedAmount(playerBb));
        Assert.Equal(new Chips(500), pot.GetRefundAmount(playerBu));
        Assert.Equal(new Chips(1000), pot.GetPreviousPostedAmount(playerBu));
        Assert.Equal(new Chips(1750), pot.GetTotalAmount());
    }

    [Theory]
    [InlineData(499)]
    [InlineData(501)]
    public void TestRefundWithWrongAmount(int amount)
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1,
            stake: 500
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2,
            stake: 250
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 6
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(25)));
        pot.CommitDecision(playerSb, new Decision(DecisionType.RaiseTo, new Chips(120)));
        pot.CommitDecision(playerBb, new Decision(DecisionType.RaiseTo, new Chips(250)));
        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(1000)));
        pot.CommitDecision(playerSb, new Decision(DecisionType.CallTo, new Chips(500)));
        pot.FinishStage();

        Assert.Equal(new Chips(0), pot.GetRefundAmount(playerSb));
        Assert.Equal(new Chips(500), pot.GetPreviousPostedAmount(playerSb));
        Assert.Equal(new Chips(0), pot.GetRefundAmount(playerBb));
        Assert.Equal(new Chips(250), pot.GetPreviousPostedAmount(playerBb));
        Assert.Equal(new Chips(500), pot.GetRefundAmount(playerBu));
        Assert.Equal(new Chips(1000), pot.GetPreviousPostedAmount(playerBu));
        Assert.Equal(new Chips(1750), pot.GetTotalAmount());

        var exc = Assert.Throws<InvalidOperationException>(() => pot.CommitRefund(playerBu, new Chips(amount)));

        Assert.Equal("The player must refund 500 chip(s)", exc.Message);
        Assert.Equal(new Chips(0), pot.GetRefundAmount(playerSb));
        Assert.Equal(new Chips(500), pot.GetPreviousPostedAmount(playerSb));
        Assert.Equal(new Chips(0), pot.GetRefundAmount(playerBb));
        Assert.Equal(new Chips(250), pot.GetPreviousPostedAmount(playerBb));
        Assert.Equal(new Chips(500), pot.GetRefundAmount(playerBu));
        Assert.Equal(new Chips(1000), pot.GetPreviousPostedAmount(playerBu));
        Assert.Equal(new Chips(1750), pot.GetTotalAmount());
    }

    [Fact]
    public void TestCommitWinWithoutShowdown()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 6
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(25)));
        pot.CommitDecision(playerSb, new Decision(DecisionType.RaiseTo, new Chips(120)));
        pot.CommitDecision(playerBb, new Decision(DecisionType.Fold, new Chips(0)));
        pot.CommitDecision(playerBu, new Decision(DecisionType.Fold, new Chips(0)));
        pot.FinishStage();
        pot.CommitRefund(playerSb, new Chips(95));

        pot.CommitWinWithoutShowdown(playerSb, new Chips(60));

        Assert.Equal(new Chips(0), pot.GetTotalAmount());
        Assert.Equal(new Chips(1035), playerSb.Stake);
    }

    [Theory]
    [InlineData(59)]
    [InlineData(61)]
    public void TestCommitWinWithoutShowdownWithWrongAmount(int amount)
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 6
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(25)));
        pot.CommitDecision(playerSb, new Decision(DecisionType.RaiseTo, new Chips(120)));
        pot.CommitDecision(playerBb, new Decision(DecisionType.Fold, new Chips(0)));
        pot.CommitDecision(playerBu, new Decision(DecisionType.Fold, new Chips(0)));
        pot.FinishStage();
        pot.CommitRefund(playerSb, new Chips(95));

        var exc = Assert.Throws<InvalidOperationException>(() => pot.CommitWinWithoutShowdown(playerSb, new Chips(amount)));

        Assert.Equal("The player must win 60 chip(s)", exc.Message);
        Assert.Equal(new Chips(60), pot.GetTotalAmount());
    }

    [Fact]
    public void TestCommitWinAtShowdown()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 6
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(25)));
        pot.CommitDecision(playerSb, new Decision(DecisionType.RaiseTo, new Chips(120)));
        pot.CommitDecision(playerBb, new Decision(DecisionType.Fold, new Chips(0)));
        pot.CommitDecision(playerBu, new Decision(DecisionType.CallTo, new Chips(120)));
        pot.FinishStage();

        var sidePot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerSb.Nickname, new Chips(120)),
            new KeyValuePair<Nickname, Chips>(playerBb.Nickname, new Chips(10)),
            new KeyValuePair<Nickname, Chips>(playerBu.Nickname, new Chips(120)),
        ]);
        var winPot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerBu.Nickname, new Chips(250)),
        ]);

        pot.CommitWinAtShowdown([playerBu], sidePot, winPot);

        Assert.Equal(new Chips(0), pot.GetTotalAmount());
        Assert.Equal(new Chips(880), playerSb.Stake);
        Assert.Equal(new Chips(1130), playerBu.Stake);
    }

    [Fact]
    public void TestCommitWinAtShowdownWithSplit()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 6,
            stake: 999  // The poorest player takes the remainder
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(25)));
        pot.CommitDecision(playerSb, new Decision(DecisionType.Fold, new Chips(0)));
        pot.CommitDecision(playerBb, new Decision(DecisionType.RaiseTo, new Chips(130)));
        pot.CommitDecision(playerBu, new Decision(DecisionType.CallTo, new Chips(130)));
        pot.FinishStage();

        var sidePot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerSb.Nickname, new Chips(5)),
            new KeyValuePair<Nickname, Chips>(playerBb.Nickname, new Chips(130)),
            new KeyValuePair<Nickname, Chips>(playerBu.Nickname, new Chips(130)),
        ]);
        var winPot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerBb.Nickname, new Chips(132)),
            new KeyValuePair<Nickname, Chips>(playerBu.Nickname, new Chips(133)),
        ]);

        pot.CommitWinAtShowdown([playerBb, playerBu], sidePot, winPot);

        Assert.Equal(new Chips(0), pot.GetTotalAmount());
        Assert.Equal(new Chips(1002), playerBb.Stake);
        Assert.Equal(new Chips(1002), playerBu.Stake);
    }

    [Fact]
    public void TestCommitWinAtShowdownWithSidePot()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1,
            stake: 100
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 6
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(25)));
        pot.CommitDecision(playerSb, new Decision(DecisionType.RaiseTo, new Chips(100)));
        pot.CommitDecision(playerBb, new Decision(DecisionType.RaiseTo, new Chips(240)));
        pot.CommitDecision(playerBu, new Decision(DecisionType.CallTo, new Chips(240)));
        pot.FinishStage();

        var sidePot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerSb.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerBb.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerBu.Nickname, new Chips(100)),
        ]);
        var winPot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerSb.Nickname, new Chips(300)),
        ]);

        pot.CommitWinAtShowdown([playerSb], sidePot, winPot);

        Assert.Equal(new Chips(280), pot.GetTotalAmount());
        Assert.Equal(new Chips(300), playerSb.Stake);
        Assert.Equal(new Chips(760), playerBb.Stake);
        Assert.Equal(new Chips(760), playerBu.Stake);

        sidePot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerBb.Nickname, new Chips(140)),
            new KeyValuePair<Nickname, Chips>(playerBu.Nickname, new Chips(140)),
        ]);
        winPot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerBb.Nickname, new Chips(280)),
        ]);

        pot.CommitWinAtShowdown([playerBb], sidePot, winPot);

        Assert.Equal(new Chips(0), pot.GetTotalAmount());
        Assert.Equal(new Chips(300), playerSb.Stake);
        Assert.Equal(new Chips(1040), playerBb.Stake);
        Assert.Equal(new Chips(760), playerBu.Stake);
    }

    [Fact]
    public void TestCommitWinAtShowdownWithMultipleSidePotsAndSplits()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1,
            stake: 100
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2,
            stake: 200
        );
        var playerUtg1 = CreatePlayer(
            nickname: "UnderTheGun1",
            seat: 3,
            stake: 300
        );
        var playerUtg2 = CreatePlayer(
            nickname: "UnderTheGun2",
            seat: 4,
            stake: 400
        );
        var playerUtg3 = CreatePlayer(
            nickname: "UnderTheGun3",
            seat: 5,
            stake: 500
        );
        var playerEp = CreatePlayer(
            nickname: "Early",
            seat: 6,
            stake: 600
        );
        var playerMp = CreatePlayer(
            nickname: "Middle",
            seat: 7,
            stake: 700
        );
        var playerCo = CreatePlayer(
            nickname: "CutOff",
            seat: 8,
            stake: 800
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 9,
            stake: 900
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerUtg1.Connect();
        playerUtg2.Connect();
        playerUtg3.Connect();
        playerEp.Connect();
        playerMp.Connect();
        playerCo.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerUtg1, new Decision(DecisionType.RaiseTo, new Chips(300)));
        pot.CommitDecision(playerUtg2, new Decision(DecisionType.RaiseTo, new Chips(400)));
        pot.CommitDecision(playerUtg3, new Decision(DecisionType.RaiseTo, new Chips(500)));
        pot.CommitDecision(playerEp, new Decision(DecisionType.RaiseTo, new Chips(600)));
        pot.CommitDecision(playerMp, new Decision(DecisionType.RaiseTo, new Chips(700)));
        pot.CommitDecision(playerCo, new Decision(DecisionType.RaiseTo, new Chips(800)));
        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(900)));
        pot.CommitDecision(playerSb, new Decision(DecisionType.CallTo, new Chips(100)));
        pot.CommitDecision(playerBb, new Decision(DecisionType.CallTo, new Chips(200)));
        pot.FinishStage();
        pot.CommitRefund(playerBu, new Chips(100));

        var sidePot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerSb.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerBb.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerUtg1.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerUtg2.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerUtg3.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerEp.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerMp.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerCo.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerBu.Nickname, new Chips(100)),
        ]);
        var winPot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerSb.Nickname, new Chips(900)),
        ]);

        pot.CommitWinAtShowdown([playerSb], sidePot, winPot);

        Assert.Equal(new Chips(3500), pot.GetTotalAmount());
        Assert.Equal(new Chips(900), playerSb.Stake);

        sidePot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerBb.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerUtg1.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerUtg2.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerUtg3.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerEp.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerMp.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerCo.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerBu.Nickname, new Chips(100)),
        ]);
        winPot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerBb.Nickname, new Chips(267)),
            new KeyValuePair<Nickname, Chips>(playerUtg1.Nickname, new Chips(267)),
            new KeyValuePair<Nickname, Chips>(playerUtg2.Nickname, new Chips(266)),
        ]);

        pot.CommitWinAtShowdown([playerBb, playerUtg1, playerUtg2], sidePot, winPot);

        Assert.Equal(new Chips(2700), pot.GetTotalAmount());
        Assert.Equal(new Chips(267), playerBb.Stake);
        Assert.Equal(new Chips(267), playerUtg1.Stake);
        Assert.Equal(new Chips(266), playerUtg2.Stake);

        sidePot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerUtg1.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerUtg2.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerUtg3.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerEp.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerMp.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerCo.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerBu.Nickname, new Chips(100)),
        ]);
        winPot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerUtg1.Nickname, new Chips(350)),
            new KeyValuePair<Nickname, Chips>(playerUtg2.Nickname, new Chips(350)),
        ]);

        pot.CommitWinAtShowdown([playerUtg1, playerUtg2], sidePot, winPot);

        Assert.Equal(new Chips(2000), pot.GetTotalAmount());
        Assert.Equal(new Chips(617), playerUtg1.Stake);
        Assert.Equal(new Chips(616), playerUtg2.Stake);

        sidePot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerUtg2.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerUtg3.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerEp.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerMp.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerCo.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerBu.Nickname, new Chips(100)),
        ]);
        winPot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerUtg2.Nickname, new Chips(600)),
        ]);

        pot.CommitWinAtShowdown([playerUtg2], sidePot, winPot);

        Assert.Equal(new Chips(1400), pot.GetTotalAmount());
        Assert.Equal(new Chips(1216), playerUtg2.Stake);

        sidePot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerUtg3.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerEp.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerMp.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerCo.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerBu.Nickname, new Chips(100)),
        ]);
        winPot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerUtg3.Nickname, new Chips(500)),
        ]);

        pot.CommitWinAtShowdown([playerUtg3], sidePot, winPot);

        Assert.Equal(new Chips(900), pot.GetTotalAmount());
        Assert.Equal(new Chips(500), playerUtg3.Stake);

        sidePot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerEp.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerMp.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerCo.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerBu.Nickname, new Chips(100)),
        ]);
        winPot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerEp.Nickname, new Chips(400)),
        ]);

        pot.CommitWinAtShowdown([playerEp], sidePot, winPot);

        Assert.Equal(new Chips(500), pot.GetTotalAmount());
        Assert.Equal(new Chips(400), playerEp.Stake);

        sidePot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerMp.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerCo.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerBu.Nickname, new Chips(100)),
        ]);
        winPot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerMp.Nickname, new Chips(150)),
            new KeyValuePair<Nickname, Chips>(playerCo.Nickname, new Chips(150)),
        ]);

        pot.CommitWinAtShowdown([playerMp, playerCo], sidePot, winPot);

        Assert.Equal(new Chips(200), pot.GetTotalAmount());
        Assert.Equal(new Chips(150), playerMp.Stake);
        Assert.Equal(new Chips(150), playerCo.Stake);

        sidePot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerCo.Nickname, new Chips(100)),
            new KeyValuePair<Nickname, Chips>(playerBu.Nickname, new Chips(100)),
        ]);
        winPot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerCo.Nickname, new Chips(200)),
        ]);

        pot.CommitWinAtShowdown([playerCo], sidePot, winPot);

        Assert.Equal(new Chips(0), pot.GetTotalAmount());
        Assert.Equal(new Chips(350), playerCo.Stake);
    }

    [Theory]
    [InlineData(249)]
    [InlineData(251)]
    public void TestCommitWinAtShowdownWithWrongAmount(int amount)
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 6
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(25)));
        pot.CommitDecision(playerSb, new Decision(DecisionType.RaiseTo, new Chips(120)));
        pot.CommitDecision(playerBb, new Decision(DecisionType.Fold, new Chips(0)));
        pot.CommitDecision(playerBu, new Decision(DecisionType.CallTo, new Chips(120)));
        pot.FinishStage();

        var sidePot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerSb.Nickname, new Chips(120)),
            new KeyValuePair<Nickname, Chips>(playerBb.Nickname, new Chips(10)),
            new KeyValuePair<Nickname, Chips>(playerBu.Nickname, new Chips(120)),
        ]);
        var winPot = new SidePot([
            new KeyValuePair<Nickname, Chips>(playerBu.Nickname, new Chips(amount)),
        ]);

        var exc = Assert.Throws<InvalidOperationException>(() =>
        {
            pot.CommitWinAtShowdown([playerBu], sidePot, winPot);
        });

        Assert.Equal("The player(s) must win 250 chip(s)", exc.Message);
        Assert.Equal(new Chips(250), pot.GetTotalAmount());
    }

    [Fact]
    public void TestRepresentation()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 6
        );
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();

        var pot = new NoLimitPot(
            smallBlind: new Chips(5),
            bigBlind: new Chips(10)
        );

        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CommitDecision(playerBu, new Decision(DecisionType.RaiseTo, new Chips(25)));

        Assert.Equal("NoLimitPot, 40 chip(s)", $"{pot}");
    }

    private BasePot CreatePot(int smallBlind = 5, int bigBlind = 10)
    {
        return new NoLimitPot(new Chips(smallBlind), new Chips(bigBlind));
    }

    private Player CreatePlayer(string nickname, int seat, int stake = 1000)
    {
        return new Player(
            nickname: new Nickname(nickname),
            seat: new Seat(seat),
            stake: new Chips(stake)
        );
    }
}
