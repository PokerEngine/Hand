using Domain.Entity;
using Domain.Event;
using Domain.Service.Dealer;
using Domain.Test.Service.Evaluator;
using Domain.Test.Service.Randomizer;
using Domain.ValueObject;

namespace Domain.Test.Service.Dealer;

public abstract class BaseBettingDealerTest
{
    protected readonly StubRandomizer Randomizer = new();
    protected readonly StubEvaluator Evaluator = new();

    protected Pot CreatePot(int minBet = 10)
    {
        return new Pot(minBet: new Chips(minBet));
    }

    protected Table CreateTable(List<Player> players, int smallBlindSeat, int bigBlindSeat, int buttonSeat)
    {
        return new Table(
            players: players,
            positions: new()
            {
                SmallBlind = new Seat(smallBlindSeat),
                BigBlind = new Seat(bigBlindSeat),
                Button = new Seat(buttonSeat),
                Max = new Seat(6)
            }
        );
    }

    protected Player CreatePlayer(string nickname, int seat, int stack = 1000)
    {
        return new Player(
            nickname: new Nickname(nickname),
            seat: new Seat(seat),
            stack: new Chips(stack)
        );
    }

    protected BaseDeck CreateDeck()
    {
        return new StandardDeck();
    }
}

public class NoLimitBettingDealerTest : BaseBettingDealerTest
{
    [Fact]
    public void Start_Preflop3Max_ShouldRequestPlayerActionNextToBigBlind()
    {
        // Arrange
        var dealer = new NoLimitBettingDealer();
        var pot = CreatePot();
        var playerA = CreatePlayer("Alice", 1);
        var playerB = CreatePlayer("Bobby", 2);
        var playerC = CreatePlayer("Charlie", 6);
        var table = CreateTable(
            players: [playerA, playerB, playerC],
            smallBlindSeat: 1,
            bigBlindSeat: 2,
            buttonSeat: 3
        );
        var deck = CreateDeck();

        playerA.Post(5);
        pot.PostBlind("Alice", 5);
        playerB.Post(10);
        pot.PostBlind("Bobby", 10);

        // Act
        var events = dealer.Start(
            rules: CreateRules(),
            table: table,
            pot: pot,
            deck: deck,
            randomizer: Randomizer,
            evaluator: Evaluator
        ).ToList();

        // Assert
        Assert.Equal(2, events.Count);
        Assert.IsType<StageStartedEvent>(events[0]);
        var requestedEvent = Assert.IsType<PlayerActionRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("Charlie"), requestedEvent.Nickname);
        Assert.True(requestedEvent.FoldIsAvailable);
        Assert.False(requestedEvent.CheckIsAvailable);
        Assert.True(requestedEvent.CallIsAvailable);
        Assert.Equal(new Chips(10), requestedEvent.CallToAmount);
        Assert.True(requestedEvent.RaiseIsAvailable);
        Assert.Equal(new Chips(20), requestedEvent.MinRaiseToAmount);
        Assert.Equal(new Chips(1000), requestedEvent.MaxRaiseToAmount);
    }

    [Fact]
    public void Start_PreflopHeadsUp_ShouldRequestPlayerActionNextToBigBlind()
    {
        // Arrange
        var dealer = new NoLimitBettingDealer();
        var pot = CreatePot();
        var playerA = CreatePlayer("Alice", 1);
        var playerB = CreatePlayer("Bobby", 2);
        var table = CreateTable(
            players: [playerA, playerB],
            smallBlindSeat: 1,
            bigBlindSeat: 2,
            buttonSeat: 1
        );
        var deck = CreateDeck();

        playerA.Post(5);
        pot.PostBlind("Alice", 5);
        playerB.Post(10);
        pot.PostBlind("Bobby", 10);

        // Act
        var events = dealer.Start(
            rules: CreateRules(),
            table: table,
            pot: pot,
            deck: deck,
            randomizer: Randomizer,
            evaluator: Evaluator
        ).ToList();

        // Assert
        Assert.Equal(2, events.Count);
        Assert.IsType<StageStartedEvent>(events[0]);
        var requestedEvent = Assert.IsType<PlayerActionRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("Alice"), requestedEvent.Nickname);
        Assert.True(requestedEvent.FoldIsAvailable);
        Assert.False(requestedEvent.CheckIsAvailable);
        Assert.True(requestedEvent.CallIsAvailable);
        Assert.Equal(new Chips(10), requestedEvent.CallToAmount);
        Assert.True(requestedEvent.RaiseIsAvailable);
        Assert.Equal(new Chips(20), requestedEvent.MinRaiseToAmount);
        Assert.Equal(new Chips(1000), requestedEvent.MaxRaiseToAmount);
    }

    [Fact]
    public void Start_PreflopHeadsUpWithDeadButton_ShouldRequestPlayerActionNextToBigBlind()
    {
        // Arrange
        var dealer = new NoLimitBettingDealer();
        var pot = CreatePot();
        var playerA = CreatePlayer("Alice", 1);
        var playerB = CreatePlayer("Bobby", 2);
        var table = CreateTable(
            players: [playerA, playerB],
            smallBlindSeat: 1,
            bigBlindSeat: 2,
            buttonSeat: 6 // Dead button
        );
        var deck = CreateDeck();

        playerA.Post(5);
        pot.PostBlind("Alice", 5);
        playerB.Post(10);
        pot.PostBlind("Bobby", 10);

        // Act
        var events = dealer.Start(
            rules: CreateRules(),
            table: table,
            pot: pot,
            deck: deck,
            randomizer: Randomizer,
            evaluator: Evaluator
        ).ToList();

        // Assert
        Assert.Equal(2, events.Count);
        Assert.IsType<StageStartedEvent>(events[0]);
        var requestedEvent = Assert.IsType<PlayerActionRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("Alice"), requestedEvent.Nickname);
        Assert.True(requestedEvent.FoldIsAvailable);
        Assert.False(requestedEvent.CheckIsAvailable);
        Assert.True(requestedEvent.CallIsAvailable);
        Assert.Equal(new Chips(10), requestedEvent.CallToAmount);
        Assert.True(requestedEvent.RaiseIsAvailable);
        Assert.Equal(new Chips(20), requestedEvent.MinRaiseToAmount);
        Assert.Equal(new Chips(1000), requestedEvent.MaxRaiseToAmount);
    }

    [Fact]
    public void Start_Postflop3Max_ShouldRequestPlayerActionNextToButton()
    {
        // Arrange
        var dealer = new NoLimitBettingDealer();
        var pot = CreatePot();
        var playerA = CreatePlayer("Alice", 1);
        var playerB = CreatePlayer("Bobby", 2);
        var playerC = CreatePlayer("Charlie", 6);
        var table = CreateTable(
            players: [playerA, playerB, playerC],
            smallBlindSeat: 1,
            bigBlindSeat: 2,
            buttonSeat: 6
        );
        var deck = CreateDeck();

        playerA.Post(5);
        pot.PostBlind("Alice", 5);
        playerB.Post(10);
        pot.PostBlind("Bobby", 10);
        playerC.Post(25); // Raise 25
        pot.PostBet("Charlie", 25);
        playerA.Fold();
        playerB.Post(15); // Call 25
        pot.PostBet("Bobby", 15);
        pot.CollectBets();

        // Act
        var events = dealer.Start(
            rules: CreateRules(),
            table: table,
            pot: pot,
            deck: deck,
            randomizer: Randomizer,
            evaluator: Evaluator
        ).ToList();

        // Assert
        Assert.Equal(2, events.Count);
        Assert.IsType<StageStartedEvent>(events[0]);
        var requestedEvent = Assert.IsType<PlayerActionRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("Bobby"), requestedEvent.Nickname);
        Assert.True(requestedEvent.FoldIsAvailable);
        Assert.True(requestedEvent.CheckIsAvailable);
        Assert.False(requestedEvent.CallIsAvailable);
        Assert.Equal(new Chips(0), requestedEvent.CallToAmount);
        Assert.True(requestedEvent.RaiseIsAvailable);
        Assert.Equal(new Chips(10), requestedEvent.MinRaiseToAmount);
        Assert.Equal(new Chips(975), requestedEvent.MaxRaiseToAmount);
    }

    [Fact]
    public void Start_PostflopHeadsUp_ShouldRequestPlayerActionNextToButton()
    {
        // Arrange
        var dealer = new NoLimitBettingDealer();
        var pot = CreatePot();
        var playerA = CreatePlayer("Alice", 1);
        var playerB = CreatePlayer("Bobby", 2);
        var table = CreateTable(
            players: [playerA, playerB],
            smallBlindSeat: 1,
            bigBlindSeat: 2,
            buttonSeat: 1
        );
        var deck = CreateDeck();

        playerA.Post(5);
        pot.PostBlind("Alice", 5);
        playerB.Post(10);
        pot.PostBlind("Bobby", 10);
        playerA.Post(25); // Raise 30
        pot.PostBet("Alice", 25);
        playerB.Post(20); // Call 30
        pot.PostBet("Bobby", 20);
        pot.CollectBets();

        // Act
        var events = dealer.Start(
            rules: CreateRules(),
            table: table,
            pot: pot,
            deck: deck,
            randomizer: Randomizer,
            evaluator: Evaluator
        ).ToList();

        // Assert
        Assert.Equal(2, events.Count);
        Assert.IsType<StageStartedEvent>(events[0]);
        var requestedEvent = Assert.IsType<PlayerActionRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("Bobby"), requestedEvent.Nickname);
        Assert.True(requestedEvent.FoldIsAvailable);
        Assert.True(requestedEvent.CheckIsAvailable);
        Assert.False(requestedEvent.CallIsAvailable);
        Assert.Equal(new Chips(0), requestedEvent.CallToAmount);
        Assert.True(requestedEvent.RaiseIsAvailable);
        Assert.Equal(new Chips(10), requestedEvent.MinRaiseToAmount);
        Assert.Equal(new Chips(970), requestedEvent.MaxRaiseToAmount);
    }

    [Fact]
    public void Start_PostflopHeadsUpWithDeadButton_ShouldRequestPlayerActionNextToButton()
    {
        // Arrange
        var dealer = new NoLimitBettingDealer();
        var pot = CreatePot();
        var playerA = CreatePlayer("Alice", 1);
        var playerB = CreatePlayer("Bobby", 2);
        var table = CreateTable(
            players: [playerA, playerB],
            smallBlindSeat: 1,
            bigBlindSeat: 2,
            buttonSeat: 6 // Dead button
        );
        var deck = CreateDeck();

        playerA.Post(5);
        pot.PostBlind("Alice", 5);
        playerB.Post(10);
        pot.PostBlind("Bobby", 10);
        playerA.Post(25); // Raise 30
        pot.PostBet("Alice", 25);
        playerB.Post(20); // Call 30
        pot.PostBet("Bobby", 20);
        pot.CollectBets();

        // Act
        var events = dealer.Start(
            rules: CreateRules(),
            table: table,
            pot: pot,
            deck: deck,
            randomizer: Randomizer,
            evaluator: Evaluator
        ).ToList();

        // Assert
        Assert.Equal(2, events.Count);
        Assert.IsType<StageStartedEvent>(events[0]);
        var requestedEvent = Assert.IsType<PlayerActionRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("Alice"), requestedEvent.Nickname);
        Assert.True(requestedEvent.FoldIsAvailable);
        Assert.True(requestedEvent.CheckIsAvailable);
        Assert.False(requestedEvent.CallIsAvailable);
        Assert.Equal(new Chips(0), requestedEvent.CallToAmount);
        Assert.True(requestedEvent.RaiseIsAvailable);
        Assert.Equal(new Chips(10), requestedEvent.MinRaiseToAmount);
        Assert.Equal(new Chips(970), requestedEvent.MaxRaiseToAmount);
    }

    [Fact]
    public void Start_PostflopWhenEveryoneFolded_ShouldFinish()
    {
        // Arrange
        var dealer = new NoLimitBettingDealer();
        var pot = CreatePot();
        var playerA = CreatePlayer("Alice", 1);
        var playerB = CreatePlayer("Bobby", 2);
        var playerC = CreatePlayer("Charlie", 6);
        var table = CreateTable(
            players: [playerA, playerB, playerC],
            smallBlindSeat: 1,
            bigBlindSeat: 2,
            buttonSeat: 6
        );
        var deck = CreateDeck();

        playerA.Post(5);
        pot.PostBlind("Alice", 5);
        playerB.Post(10);
        pot.PostBlind("Bobby", 10);
        playerC.Post(25); // Raise 25
        pot.PostBet("Charlie", 25);
        playerA.Fold();
        playerB.Fold();
        pot.RefundBet("Charlie", 15);
        pot.CollectBets();

        // Act
        var events = dealer.Start(
            rules: CreateRules(),
            table: table,
            pot: pot,
            deck: deck,
            randomizer: Randomizer,
            evaluator: Evaluator
        ).ToList();

        // Assert
        Assert.Equal(2, events.Count);
        Assert.IsType<StageStartedEvent>(events[0]);
        Assert.IsType<StageFinishedEvent>(events[1]);
    }

    [Fact]
    public void Start_PostflopWhenEveryoneAreAllIn_ShouldFinish()
    {
        // Arrange
        var dealer = new NoLimitBettingDealer();
        var pot = CreatePot();
        var playerA = CreatePlayer("Alice", 1, 900); // Less stack than Charlie's one
        var playerB = CreatePlayer("Bobby", 2);
        var playerC = CreatePlayer("Charlie", 6);
        var table = CreateTable(
            players: [playerA, playerB, playerC],
            smallBlindSeat: 1,
            bigBlindSeat: 2,
            buttonSeat: 6
        );
        var deck = CreateDeck();

        playerA.Post(5);
        pot.PostBlind("Alice", 5);
        playerB.Post(10);
        pot.PostBlind("Bobby", 10);
        playerC.Post(25); // Raise 25
        pot.PostBet("Charlie", 25);
        playerA.Post(115); // Raise 120
        pot.PostBet("Alice", 110);
        playerB.Fold();
        playerC.Post(975); // Raise 1000 (all-in)
        pot.PostBet("Charlie", 975);
        playerA.Post(780); // Call 900 (all-in)
        pot.PostBet("Alice", 780);
        pot.RefundBet("Charlie", 100);
        pot.CollectBets();

        // Act
        var events = dealer.Start(
            rules: CreateRules(),
            table: table,
            pot: pot,
            deck: deck,
            randomizer: Randomizer,
            evaluator: Evaluator
        ).ToList();

        // Assert
        Assert.Equal(2, events.Count);
        Assert.IsType<StageStartedEvent>(events[0]);
        Assert.IsType<StageFinishedEvent>(events[1]);
    }

    private Rules CreateRules()
    {
        return new Rules
        {
            Game = Game.NoLimitHoldem,
            SmallBlind = new Chips(5),
            BigBlind = new Chips(10)
        };
    }
}

public class PotLimitBettingDealerTest : BaseBettingDealerTest
{
    [Fact]
    public void Start_Preflop3Max_ShouldRequestPlayerActionNextToBigBlind()
    {
        // Arrange
        var dealer = new PotLimitBettingDealer();
        var pot = CreatePot();
        var playerA = CreatePlayer("Alice", 1);
        var playerB = CreatePlayer("Bobby", 2);
        var playerC = CreatePlayer("Charlie", 6);
        var table = CreateTable(
            players: [playerA, playerB, playerC],
            smallBlindSeat: 1,
            bigBlindSeat: 2,
            buttonSeat: 3
        );
        var deck = CreateDeck();

        playerA.Post(5);
        pot.PostBlind("Alice", 5);
        playerB.Post(10);
        pot.PostBlind("Bobby", 10);

        // Act
        var events = dealer.Start(
            rules: CreateRules(),
            table: table,
            pot: pot,
            deck: deck,
            randomizer: Randomizer,
            evaluator: Evaluator
        ).ToList();

        // Assert
        Assert.Equal(2, events.Count);
        Assert.IsType<StageStartedEvent>(events[0]);
        var requestedEvent = Assert.IsType<PlayerActionRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("Charlie"), requestedEvent.Nickname);
        Assert.Equal(new Chips(35), requestedEvent.MaxRaiseToAmount);
    }

    [Fact]
    public void Start_PreflopHeadsUp_ShouldRequestPlayerActionNextToBigBlind()
    {
        // Arrange
        var dealer = new PotLimitBettingDealer();
        var pot = CreatePot();
        var playerA = CreatePlayer("Alice", 1);
        var playerB = CreatePlayer("Bobby", 2);
        var table = CreateTable(
            players: [playerA, playerB],
            smallBlindSeat: 1,
            bigBlindSeat: 2,
            buttonSeat: 1
        );
        var deck = CreateDeck();

        playerA.Post(5);
        pot.PostBlind("Alice", 5);
        playerB.Post(10);
        pot.PostBlind("Bobby", 10);

        // Act
        var events = dealer.Start(
            rules: CreateRules(),
            table: table,
            pot: pot,
            deck: deck,
            randomizer: Randomizer,
            evaluator: Evaluator
        ).ToList();

        // Assert
        Assert.Equal(2, events.Count);
        Assert.IsType<StageStartedEvent>(events[0]);
        var requestedEvent = Assert.IsType<PlayerActionRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("Alice"), requestedEvent.Nickname);
        Assert.Equal(new Chips(30), requestedEvent.MaxRaiseToAmount);
    }

    [Fact]
    public void Start_Postflop3Max_ShouldRequestPlayerActionNextToButton()
    {
        // Arrange
        var dealer = new PotLimitBettingDealer();
        var pot = CreatePot();
        var playerA = CreatePlayer("Alice", 1);
        var playerB = CreatePlayer("Bobby", 2);
        var playerC = CreatePlayer("Charlie", 6);
        var table = CreateTable(
            players: [playerA, playerB, playerC],
            smallBlindSeat: 1,
            bigBlindSeat: 2,
            buttonSeat: 6
        );
        var deck = CreateDeck();

        playerA.Post(5);
        pot.PostBlind("Alice", 5);
        playerB.Post(10);
        pot.PostBlind("Bobby", 10);
        playerC.Post(35); // Raise 35
        pot.PostBet("Charlie", 35);
        playerA.Fold();
        playerB.Post(25); // Call 35
        pot.PostBet("Bobby", 25);
        pot.CollectBets();

        // Act
        var events = dealer.Start(
            rules: CreateRules(),
            table: table,
            pot: pot,
            deck: deck,
            randomizer: Randomizer,
            evaluator: Evaluator
        ).ToList();

        // Assert
        Assert.Equal(2, events.Count);
        Assert.IsType<StageStartedEvent>(events[0]);
        var requestedEvent = Assert.IsType<PlayerActionRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("Bobby"), requestedEvent.Nickname);
        Assert.Equal(new Chips(75), requestedEvent.MaxRaiseToAmount);
    }

    private Rules CreateRules()
    {
        return new Rules
        {
            Game = Game.PotLimitOmaha,
            SmallBlind = new Chips(5),
            BigBlind = new Chips(10)
        };
    }
}
