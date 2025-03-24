using System.Collections.Immutable;

using Domain.Entity;
using Domain.Error;
using Domain.ValueObject;

namespace DomainTest.EntityTest;

public class NoLimitPotTest
{
    [Fact]
    public void TestInitialization()
    {
        var currentSidePot = new SidePot();
        currentSidePot = currentSidePot.Add(new Nickname("SmallBlind"), new Chips(25));
        currentSidePot = currentSidePot.Add(new Nickname("BigBlind"), new Chips(10));

        var pot = new NoLimitPot(
            smallBlind: new Chips(5),
            bigBlind: new Chips(10),
            lastDecisionNickname: new Nickname("SmallBlind"),
            lastRaiseNickname: new Nickname("SmallBlind"),
            lastRaiseStep: new Chips(15),
            currentDecisionNicknames: ImmutableHashSet.Create(new Nickname("SmallBlind")),
            currentSidePot: currentSidePot,
            previousSidePot: new SidePot()
        );

        Assert.Equal(new Chips(5), pot.SmallBlind);
        Assert.Equal(new Chips(10), pot.BigBlind);
        Assert.Equal(new Nickname("SmallBlind"), pot.LastDecisionNickname);
        Assert.Equal(new Nickname("SmallBlind"), pot.LastRaiseNickname);
        Assert.Equal(new Chips(15), pot.LastRaiseStep);
        Assert.Equal(ImmutableHashSet.Create(new Nickname("SmallBlind")), pot.CurrentDecisionNicknames);
        Assert.Equal(currentSidePot, pot.CurrentSidePot);
        Assert.Empty(pot.PreviousSidePot);
        Assert.Equal(new Chips(35), pot.GetTotalAmount());
    }

    [Fact]
    public void TestPostSmallBlind()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();

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
            position: Position.SmallBlind,
            stake: 2
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();

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
    public void TestPostSmallBlindWithWrongAmount(Chips amount)
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();

        var exc = Assert.Throws<NotAvailableError>(() => pot.PostSmallBlind(playerSb, amount));

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
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
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
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind,
            stake: 2
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
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
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();

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
    public void TestPostBigBlindWithWrongAmount(Chips amount)
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));

        var exc = Assert.Throws<NotAvailableError>(() => pot.PostBigBlind(playerBb, amount));

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
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
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

        pot.Fold(playerSb);

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
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.Fold(playerSb);

        Assert.False(pot.DecisionIsAvailable(playerBb));

        var exc = Assert.Throws<NotAvailableError>(() => pot.Fold(playerBb));

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
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CallTo(playerSb, new Chips(10));

        Assert.True(pot.DecisionIsAvailable(playerBb));
        Assert.False(pot.FoldIsAvailable(playerBb));
        Assert.True(pot.CheckIsAvailable(playerBb));
        Assert.False(pot.CallIsAvailable(playerBb));
        Assert.True(pot.RaiseIsAvailable(playerBb));
        Assert.Equal(new Chips(20), pot.GetMinRaiseToAmount(playerSb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerSb));

        var exc = Assert.Throws<NotAvailableError>(() => pot.Fold(playerBb));

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
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CallTo(playerSb, new Chips(10));

        Assert.True(pot.DecisionIsAvailable(playerBb));
        Assert.False(pot.FoldIsAvailable(playerBb));
        Assert.True(pot.CheckIsAvailable(playerBb));
        Assert.False(pot.CallIsAvailable(playerBb));
        Assert.True(pot.RaiseIsAvailable(playerBb));
        Assert.Equal(new Chips(20), pot.GetMinRaiseToAmount(playerBb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerBb));

        pot.Check(playerBb);

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
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.Fold(playerSb);

        Assert.False(pot.DecisionIsAvailable(playerBb));

        var exc = Assert.Throws<NotAvailableError>(() => pot.Check(playerBb));

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
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
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

        var exc = Assert.Throws<NotAvailableError>(() => pot.Check(playerSb));

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
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CallTo(playerSb, new Chips(10));

        Assert.False(pot.DecisionIsAvailable(playerSb));

        var exc = Assert.Throws<NotAvailableError>(() => pot.Check(playerSb));

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
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
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

        pot.CallTo(playerSb, new Chips(10));

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
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind,
            stake: 19
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.RaiseTo(playerSb, new Chips(20));

        Assert.True(pot.DecisionIsAvailable(playerBb));
        Assert.True(pot.FoldIsAvailable(playerBb));
        Assert.False(pot.CheckIsAvailable(playerBb));
        Assert.True(pot.CallIsAvailable(playerBb));
        Assert.Equal(new Chips(19), pot.GetCallToAmount(playerBb));
        Assert.False(pot.RaiseIsAvailable(playerBb));

        pot.CallTo(playerBb, new Chips(19));

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
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.Fold(playerSb);

        Assert.False(pot.DecisionIsAvailable(playerBb));

        var exc = Assert.Throws<NotAvailableError>(() => pot.CallTo(playerBb, new Chips(10)));

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
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CallTo(playerSb, new Chips(10));

        Assert.True(pot.DecisionIsAvailable(playerBb));
        Assert.False(pot.FoldIsAvailable(playerBb));
        Assert.True(pot.CheckIsAvailable(playerBb));
        Assert.False(pot.CallIsAvailable(playerBb));
        Assert.True(pot.RaiseIsAvailable(playerBb));
        Assert.Equal(new Chips(20), pot.GetMinRaiseToAmount(playerBb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerBb));

        var exc = Assert.Throws<NotAvailableError>(() => pot.CallTo(playerBb, new Chips(10)));

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
    public void TestCallToWrongAmount(Chips amount)
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
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

        var exc = Assert.Throws<NotAvailableError>(() => pot.CallTo(playerSb, amount));

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
            position: Position.SmallBlind,
            stake: 9
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
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

        var exc = Assert.Throws<NotAvailableError>(() => pot.CallTo(playerSb, new Chips(8)));

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
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
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

        pot.RaiseTo(playerSb, new Chips(20));

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
            position: Position.SmallBlind,
            stake: 19
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
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

        pot.RaiseTo(playerSb, new Chips(19));

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
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.Fold(playerSb);

        Assert.False(pot.DecisionIsAvailable(playerBb));

        var exc = Assert.Throws<NotAvailableError>(() => pot.RaiseTo(playerBb, new Chips(20)));

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
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.CallTo(playerSb, new Chips(10));

        Assert.True(pot.DecisionIsAvailable(playerBb));
        Assert.False(pot.FoldIsAvailable(playerBb));
        Assert.True(pot.CheckIsAvailable(playerBb));
        Assert.False(pot.CallIsAvailable(playerBb));
        Assert.True(pot.RaiseIsAvailable(playerBb));
        Assert.Equal(new Chips(20), pot.GetMinRaiseToAmount(playerBb));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerBb));

        pot.RaiseTo(playerBb, new Chips(20));

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
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
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

        var exc = Assert.Throws<NotAvailableError>(() => pot.RaiseTo(playerSb, new Chips(19)));

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
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
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

        var exc = Assert.Throws<NotAvailableError>(() => pot.RaiseTo(playerSb, new Chips(1001)));

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
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind,
            stake: 19
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.RaiseTo(playerSb, new Chips(20));

        Assert.True(pot.DecisionIsAvailable(playerBb));
        Assert.True(pot.FoldIsAvailable(playerBb));
        Assert.False(pot.CheckIsAvailable(playerBb));
        Assert.True(pot.CallIsAvailable(playerBb));
        Assert.Equal(new Chips(19), pot.GetCallToAmount(playerBb));
        Assert.False(pot.RaiseIsAvailable(playerBb));

        var exc = Assert.Throws<NotAvailableError>(() => pot.RaiseTo(playerBb, new Chips(19)));

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
            position: Position.SmallBlind,
            stake: 19
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
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

        var exc = Assert.Throws<NotAvailableError>(() => pot.RaiseTo(playerSb, new Chips(18)));

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
            position: Position.SmallBlind,
            stake: 40
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            position: Position.Button
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.RaiseTo(playerBu, new Chips(25));
        pot.RaiseTo(playerSb, new Chips(40));  // Is considered a raise
        pot.CallTo(playerBb, new Chips(40));

        Assert.True(pot.DecisionIsAvailable(playerBu));
        Assert.True(pot.FoldIsAvailable(playerBu));
        Assert.False(pot.CheckIsAvailable(playerBu));
        Assert.True(pot.CallIsAvailable(playerBu));
        Assert.Equal(new Chips(40), pot.GetCallToAmount(playerBu));
        Assert.True(pot.RaiseIsAvailable(playerBu));
        Assert.Equal(new Chips(55), pot.GetMinRaiseToAmount(playerBu));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerBu));

        pot.RaiseTo(playerBu, new Chips(55));

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
            position: Position.SmallBlind,
            stake: 39
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            position: Position.Button
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.RaiseTo(playerBu, new Chips(25));
        pot.RaiseTo(playerSb, new Chips(39));  // Is not considered a raise because the current step is 15
        pot.CallTo(playerBb, new Chips(39));

        Assert.True(pot.DecisionIsAvailable(playerBu));
        Assert.True(pot.FoldIsAvailable(playerBu));
        Assert.False(pot.CheckIsAvailable(playerBu));
        Assert.True(pot.CallIsAvailable(playerBu));
        Assert.Equal(new Chips(39), pot.GetCallToAmount(playerBu));
        Assert.False(pot.RaiseIsAvailable(playerBu));

        var exc = Assert.Throws<NotAvailableError>(() => pot.RaiseTo(playerBb, new Chips(55)));

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
            position: Position.SmallBlind,
            stake: 39
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            position: Position.Button
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.RaiseTo(playerBu, new Chips(25));
        pot.RaiseTo(playerSb, new Chips(39));  // Is not considered a raise because the current step is 15
        pot.RaiseTo(playerBb, new Chips(54));  // Is considered a raise

        Assert.True(pot.DecisionIsAvailable(playerBu));
        Assert.True(pot.FoldIsAvailable(playerBu));
        Assert.False(pot.CheckIsAvailable(playerBu));
        Assert.True(pot.CallIsAvailable(playerBu));
        Assert.Equal(new Chips(54), pot.GetCallToAmount(playerBu));
        Assert.True(pot.RaiseIsAvailable(playerBu));
        Assert.Equal(new Chips(69), pot.GetMinRaiseToAmount(playerBu));
        Assert.Equal(new Chips(1000), pot.GetMaxRaiseToAmount(playerBu));

        pot.RaiseTo(playerBu, new Chips(69));

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
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            position: Position.Button
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.RaiseTo(playerBu, new Chips(25));
        pot.RaiseTo(playerSb, new Chips(120));
        pot.Fold(playerBb);
        pot.CallTo(playerBu, new Chips(120));

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
        Assert.Null(pot.LastRaiseNickname);
        Assert.Equal(pot.BigBlind, pot.LastRaiseStep);
        Assert.Empty(pot.CurrentDecisionNicknames);
    }

    [Fact]
    public void TestRefundWhenAllOpponentsFolded()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            position: Position.Button
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.RaiseTo(playerBu, new Chips(25));
        pot.RaiseTo(playerSb, new Chips(120));
        pot.Fold(playerBb);
        pot.Fold(playerBu);
        pot.FinishStage();

        Assert.Equal(new Chips(95), pot.GetRefundAmount(playerSb));
        Assert.Equal(new Chips(120), pot.GetPreviousPostedAmount(playerSb));
        Assert.Equal(new Chips(155), pot.GetTotalAmount());

        pot.Refund(playerSb, new Chips(95));

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
            position: Position.SmallBlind,
            stake: 500
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind,
            stake: 250
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            position: Position.Button
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.RaiseTo(playerBu, new Chips(25));
        pot.RaiseTo(playerSb, new Chips(120));
        pot.RaiseTo(playerBb, new Chips(250));
        pot.RaiseTo(playerBu, new Chips(1000));
        pot.CallTo(playerSb, new Chips(500));
        pot.FinishStage();

        Assert.Equal(new Chips(0), pot.GetRefundAmount(playerSb));
        Assert.Equal(new Chips(500), pot.GetPreviousPostedAmount(playerSb));
        Assert.Equal(new Chips(0), pot.GetRefundAmount(playerBb));
        Assert.Equal(new Chips(250), pot.GetPreviousPostedAmount(playerBb));
        Assert.Equal(new Chips(500), pot.GetRefundAmount(playerBu));
        Assert.Equal(new Chips(1000), pot.GetPreviousPostedAmount(playerBu));
        Assert.Equal(new Chips(1750), pot.GetTotalAmount());

        pot.Refund(playerBu, new Chips(500));

        Assert.Equal(new Chips(0), pot.GetRefundAmount(playerBu));
        Assert.Equal(new Chips(500), pot.GetPreviousPostedAmount(playerBu));
        Assert.Equal(new Chips(1250), pot.GetTotalAmount());
    }

    [Fact]
    public void TestRefundWhenPostedLessAmount()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind,
            stake: 500
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind,
            stake: 250
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            position: Position.Button
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.RaiseTo(playerBu, new Chips(25));
        pot.RaiseTo(playerSb, new Chips(120));
        pot.RaiseTo(playerBb, new Chips(250));
        pot.RaiseTo(playerBu, new Chips(1000));
        pot.CallTo(playerSb, new Chips(500));
        pot.FinishStage();

        Assert.Equal(new Chips(0), pot.GetRefundAmount(playerSb));
        Assert.Equal(new Chips(500), pot.GetPreviousPostedAmount(playerSb));
        Assert.Equal(new Chips(0), pot.GetRefundAmount(playerBb));
        Assert.Equal(new Chips(250), pot.GetPreviousPostedAmount(playerBb));
        Assert.Equal(new Chips(500), pot.GetRefundAmount(playerBu));
        Assert.Equal(new Chips(1000), pot.GetPreviousPostedAmount(playerBu));
        Assert.Equal(new Chips(1750), pot.GetTotalAmount());

        var exc = Assert.Throws<NotAvailableError>(() => pot.Refund(playerSb, new Chips(0)));

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
    public void TestRefundWithWrongAmount(Chips amount)
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind,
            stake: 500
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind,
            stake: 250
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            position: Position.Button
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.RaiseTo(playerBu, new Chips(25));
        pot.RaiseTo(playerSb, new Chips(120));
        pot.RaiseTo(playerBb, new Chips(250));
        pot.RaiseTo(playerBu, new Chips(1000));
        pot.CallTo(playerSb, new Chips(500));
        pot.FinishStage();

        Assert.Equal(new Chips(0), pot.GetRefundAmount(playerSb));
        Assert.Equal(new Chips(500), pot.GetPreviousPostedAmount(playerSb));
        Assert.Equal(new Chips(0), pot.GetRefundAmount(playerBb));
        Assert.Equal(new Chips(250), pot.GetPreviousPostedAmount(playerBb));
        Assert.Equal(new Chips(500), pot.GetRefundAmount(playerBu));
        Assert.Equal(new Chips(1000), pot.GetPreviousPostedAmount(playerBu));
        Assert.Equal(new Chips(1750), pot.GetTotalAmount());

        var exc = Assert.Throws<NotAvailableError>(() => pot.Refund(playerBu, amount));

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
    public void TestWinWithoutShowdown()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            position: Position.Button
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.RaiseTo(playerBu, new Chips(25));
        pot.RaiseTo(playerSb, new Chips(120));
        pot.Fold(playerBb);
        pot.Fold(playerBu);
        pot.FinishStage();
        pot.Refund(playerSb, new Chips(95));

        pot.WinWithoutShowdown(playerSb, new Chips(60));

        Assert.Equal(new Chips(0), pot.GetTotalAmount());
        Assert.Equal(new Chips(1035), playerSb.Stake);
    }

    [Theory]
    [InlineData(59)]
    [InlineData(61)]
    public void TestWinWithoutShowdownWithWrongAmount(Chips amount)
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            position: Position.Button
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.RaiseTo(playerBu, new Chips(25));
        pot.RaiseTo(playerSb, new Chips(120));
        pot.Fold(playerBb);
        pot.Fold(playerBu);
        pot.FinishStage();
        pot.Refund(playerSb, new Chips(95));

        var exc = Assert.Throws<NotAvailableError>(() => pot.WinWithoutShowdown(playerSb, amount));

        Assert.Equal("The player must win 60 chip(s)", exc.Message);
        Assert.Equal(new Chips(60), pot.GetTotalAmount());
    }

    [Fact]
    public void TestWinAtShowdown()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            position: Position.Button
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.RaiseTo(playerBu, new Chips(25));
        pot.RaiseTo(playerSb, new Chips(120));
        pot.Fold(playerBb);
        pot.CallTo(playerBu, new Chips(120));
        pot.FinishStage();

        var comboSb = new Combo(ComboType.OnePair, 100);
        var comboBu = new Combo(ComboType.OnePair, 200);

        pot.WinAtShowdown([
            (playerSb, comboSb, new Chips(0)),
            (playerBu, comboBu, new Chips(250)),
        ]);

        Assert.Equal(new Chips(0), pot.GetTotalAmount());
        Assert.Equal(new Chips(880), playerSb.Stake);
        Assert.Equal(new Chips(1130), playerBu.Stake);
    }

    [Fact]
    public void TestWinAtShowdownWithSplit()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            position: Position.Button,
            stake: 999  // The poorest player takes the remainder
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.RaiseTo(playerBu, new Chips(25));
        pot.Fold(playerSb);
        pot.RaiseTo(playerBb, new Chips(130));
        pot.CallTo(playerBu, new Chips(130));
        pot.FinishStage();

        var comboBb = new Combo(ComboType.HighCard, 100);
        var comboBu = new Combo(ComboType.HighCard, 100);

        pot.WinAtShowdown([
            (playerBb, comboBb, new Chips(132)),
            (playerBu, comboBu, new Chips(133)),
        ]);

        Assert.Equal(new Chips(0), pot.GetTotalAmount());
        Assert.Equal(new Chips(1002), playerBb.Stake);
        Assert.Equal(new Chips(1002), playerBu.Stake);
    }

    [Fact]
    public void TestWinAtShowdownWithSidePot()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind,
            stake: 100
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            position: Position.Button
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.RaiseTo(playerBu, new Chips(25));
        pot.RaiseTo(playerSb, new Chips(100));
        pot.RaiseTo(playerBb, new Chips(240));
        pot.CallTo(playerBu, new Chips(240));
        pot.FinishStage();

        var comboSb = new Combo(ComboType.TwoPair, 300);
        var comboBb = new Combo(ComboType.OnePair, 200);
        var comboBu = new Combo(ComboType.HighCard, 100);

        pot.WinAtShowdown([
            (playerSb, comboSb, new Chips(300)),
            (playerBb, comboBb, new Chips(280)),
            (playerBu, comboBu, new Chips(0)),
        ]);

        Assert.Equal(new Chips(0), pot.GetTotalAmount());
        Assert.Equal(new Chips(300), playerSb.Stake);
        Assert.Equal(new Chips(1040), playerBb.Stake);
        Assert.Equal(new Chips(760), playerBu.Stake);
    }

    [Fact]
    public void TestWinAtShowdownWithMultipleSidePotsAndSplits()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind,
            stake: 100
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind,
            stake: 200
        );
        var playerUtg1 = CreatePlayer(
            nickname: "UnderTheGun1",
            position: Position.UnderTheGun1,
            stake: 300
        );
        var playerUtg2 = CreatePlayer(
            nickname: "UnderTheGun2",
            position: Position.UnderTheGun2,
            stake: 400
        );
        var playerUtg3 = CreatePlayer(
            nickname: "UnderTheGun3",
            position: Position.UnderTheGun3,
            stake: 500
        );
        var playerEp = CreatePlayer(
            nickname: "Early",
            position: Position.Early,
            stake: 600
        );
        var playerMp = CreatePlayer(
            nickname: "Middle",
            position: Position.Middle,
            stake: 700
        );
        var playerCo = CreatePlayer(
            nickname: "CutOff",
            position: Position.CutOff,
            stake: 800
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            position: Position.Button,
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
        pot.RaiseTo(playerUtg1, new Chips(300));
        pot.RaiseTo(playerUtg2, new Chips(400));
        pot.RaiseTo(playerUtg3, new Chips(500));
        pot.RaiseTo(playerEp, new Chips(600));
        pot.RaiseTo(playerMp, new Chips(700));
        pot.RaiseTo(playerCo, new Chips(800));
        pot.RaiseTo(playerBu, new Chips(900));
        pot.CallTo(playerSb, new Chips(100));
        pot.CallTo(playerBb, new Chips(200));
        pot.FinishStage();
        pot.Refund(playerBu, new Chips(100));

        var comboSb = new Combo(ComboType.Flush, 600);
        var comboBb = new Combo(ComboType.Straight, 500);
        var comboUtg1 = new Combo(ComboType.Straight, 500);
        var comboUtg2 = new Combo(ComboType.Straight, 500);
        var comboUtg3 = new Combo(ComboType.Trips, 400);
        var comboEp = new Combo(ComboType.TwoPair, 300);
        var comboMp = new Combo(ComboType.OnePair, 200);
        var comboCo = new Combo(ComboType.OnePair, 200);
        var comboBu = new Combo(ComboType.HighCard, 100);

        pot.WinAtShowdown([
            (playerSb, comboSb, new Chips(900)),
            (playerBb, comboBb, new Chips(268)),
            (playerUtg1, comboUtg1, new Chips(616)),
            (playerUtg2, comboUtg2, new Chips(1216)),
            (playerUtg3, comboUtg3, new Chips(500)),
            (playerEp, comboEp, new Chips(400)),
            (playerMp, comboMp, new Chips(150)),
            (playerCo, comboCo, new Chips(350)),
            (playerBu, comboBu, new Chips(0)),
        ]);

        Assert.Equal(new Chips(0), pot.GetTotalAmount());
        Assert.Equal(new Chips(900), playerSb.Stake);
        Assert.Equal(new Chips(268), playerBb.Stake);
        Assert.Equal(new Chips(616), playerUtg1.Stake);
        Assert.Equal(new Chips(1216), playerUtg2.Stake);
        Assert.Equal(new Chips(500), playerUtg3.Stake);
        Assert.Equal(new Chips(400), playerEp.Stake);
        Assert.Equal(new Chips(150), playerMp.Stake);
        Assert.Equal(new Chips(350), playerCo.Stake);
        Assert.Equal(new Chips(100), playerBu.Stake);
    }

    [Theory]
    [InlineData(0, 249, "Player Button with combo OnePair [200] must win 250 chip(s)")]
    [InlineData(0, 251, "Player Button with combo OnePair [200] must win 250 chip(s)")]
    [InlineData(1, 249, "Player SmallBlind with combo HighCard [100] must win 0 chip(s)")]
    public void TestWinAtShowdownWithWrongAmounts(Chips amountSb, Chips amountBu, string expectedMessage)
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            position: Position.Button
        );
        var pot = CreatePot();
        playerSb.Connect();
        playerBb.Connect();
        playerBu.Connect();
        pot.PostSmallBlind(playerSb, new Chips(5));
        pot.PostBigBlind(playerBb, new Chips(10));
        pot.RaiseTo(playerBu, new Chips(25));
        pot.RaiseTo(playerSb, new Chips(120));
        pot.Fold(playerBb);
        pot.CallTo(playerBu, new Chips(120));
        pot.FinishStage();

        var comboSb = new Combo(ComboType.HighCard, 100);
        var comboBu = new Combo(ComboType.OnePair, 200);

        var exc = Assert.Throws<NotAvailableError>(() =>
        {
            pot.WinAtShowdown([
                (playerSb, comboSb, amountSb),
                (playerBu, comboBu, amountBu),
            ]);
        });

        Assert.Equal(expectedMessage, exc.Message);
        Assert.Equal(new Chips(250), pot.GetTotalAmount());
    }

    [Fact]
    public void TestRepresentation()
    {
        var pot = new NoLimitPot(
            smallBlind: new Chips(5),
            bigBlind: new Chips(10),
            lastDecisionNickname: new Nickname("SmallBlind"),
            lastRaiseNickname: new Nickname("SmallBlind"),
            lastRaiseStep: new Chips(15),
            currentDecisionNicknames: ImmutableHashSet.Create(new Nickname("SmallBlind")),
            currentSidePot: new SidePot(
                new Dictionary<Nickname, Chips>()
                {
                    {new Nickname("SmallBlind"), new Chips(25) },
                    {new Nickname("BigBlind"), new Chips(10) }
                }.ToImmutableDictionary(),
                new Chips(5)
            ),
            previousSidePot: new SidePot()
        );

        Assert.Equal("NoLimitPot, 40 chip(s)", $"{pot}");
    }

    private NoLimitPot CreatePot(int smallBlind = 5, int bigBlind = 10)
    {
        return new NoLimitPot(
            smallBlind: new Chips(smallBlind),
            bigBlind: new Chips(bigBlind),
            lastDecisionNickname: null,
            lastRaiseNickname: null,
            lastRaiseStep: new Chips(bigBlind),
            currentDecisionNicknames: [],
            currentSidePot: new SidePot(),
            previousSidePot: new SidePot()
        );
    }

    private Player CreatePlayer(string nickname, Position position, int stake = 1000)
    {
        return new Player(
            nickname: new Nickname(nickname),
            position: position,
            stake: new Chips(stake),
            holeCards: new CardSet(),
            isConnected: false,
            isFolded: false
        );
    }
}