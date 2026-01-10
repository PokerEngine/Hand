using Domain.Entity;
using Domain.Event;
using Domain.Service.Dealer;
using Domain.Test.Service.Evaluator;
using Domain.Test.Service.Randomizer;
using Domain.ValueObject;

namespace Domain.Test.Service.Dealer;

public class SettlementDealerTest
{
    private readonly StubRandomizer _randomizer = new();

    [Fact]
    public void Start_OnePlayerRemaining_ShouldMuckCards()
    {
        // Arrange
        var dealer = new SettlementDealer();
        var evaluator = new StubEvaluator();
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
        pot.PostBlind(playerA.Nickname, 5);
        playerB.Post(10);
        pot.PostBlind(playerB.Nickname, 10);
        playerA.TakeHoleCards("6s2c");
        playerB.TakeHoleCards("9s7s");
        playerC.TakeHoleCards("Ac5c");
        playerC.Post(25);
        pot.PostBet(playerC.Nickname, 25); // Raise 25
        playerA.Fold();
        playerB.Fold();
        pot.RefundBet(playerC.Nickname, 15);
        pot.CommitBets();

        // Act
        var events = dealer.Start(
            rules: CreateRules(),
            table: table,
            pot: pot,
            deck: deck,
            randomizer: _randomizer,
            evaluator: evaluator
        ).ToList();

        // Assert
        Assert.Equal(4, events.Count);
        Assert.IsType<StageIsStartedEvent>(events[0]);
        var muckEvent = Assert.IsType<HoleCardsAreMuckedEvent>(events[1]);
        Assert.Equal(playerC.Nickname, muckEvent.Nickname);
        var winEvent = Assert.IsType<WinIsCommittedEvent>(events[2]);
        Assert.Equal([playerC.Nickname], winEvent.Nicknames);
        Assert.Equal(new Chips(25), winEvent.Amount);
        Assert.IsType<StageIsFinishedEvent>(events[3]);
    }

    [Fact]
    public void Start_SeveralPlayersRemaining_LooserOutOfPositionShouldShowCards()
    {
        // Arrange
        var dealer = new SettlementDealer();
        var evaluator = new StubEvaluator();
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
        pot.PostBlind(playerA.Nickname, 5);
        playerB.Post(10);
        pot.PostBlind(playerB.Nickname, 10);
        playerA.TakeHoleCards("6s2c");
        playerB.TakeHoleCards("9s7s");
        playerC.TakeHoleCards("Ac5c");
        playerC.Post(25);
        pot.PostBet(playerC.Nickname, 25); // Raise 25
        playerA.Fold();
        playerB.Post(15);
        pot.PostBet(playerB.Nickname, 15); // Call 25
        pot.CommitBets();
        table.TakeBoardCards("Ad9d6c6dTs");

        var comboB = new Combo(ComboType.TwoPair, 10);
        var comboC = new Combo(ComboType.TwoPair, 20);
        evaluator.SetCombo(Game.NoLimitHoldem, table.BoardCards, playerB.HoleCards, comboB);
        evaluator.SetCombo(Game.NoLimitHoldem, table.BoardCards, playerC.HoleCards, comboC);

        // Act
        var events = dealer.Start(
            rules: CreateRules(),
            table: table,
            pot: pot,
            deck: deck,
            randomizer: _randomizer,
            evaluator: evaluator
        ).ToList();

        // Assert
        Assert.Equal(5, events.Count);
        Assert.IsType<StageIsStartedEvent>(events[0]);
        var showEventB = Assert.IsType<HoleCardsAreShownEvent>(events[1]);
        Assert.Equal(playerB.Nickname, showEventB.Nickname);
        Assert.Equal(playerB.HoleCards, showEventB.Cards);
        Assert.Equal(comboB, showEventB.Combo);
        var showEventC = Assert.IsType<HoleCardsAreShownEvent>(events[2]);
        Assert.Equal(playerC.Nickname, showEventC.Nickname);
        Assert.Equal(playerC.HoleCards, showEventC.Cards);
        Assert.Equal(comboC, showEventC.Combo);
        var winEvent = Assert.IsType<WinIsCommittedEvent>(events[3]);
        Assert.Equal([playerC.Nickname], winEvent.Nicknames);
        Assert.Equal(new Chips(55), winEvent.Amount);
        Assert.IsType<StageIsFinishedEvent>(events[4]);
    }

    [Fact]
    public void Start_SeveralPlayersRemaining_LooserInPositionShouldMuckCards()
    {
        // Arrange
        var dealer = new SettlementDealer();
        var evaluator = new StubEvaluator();
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
        pot.PostBlind(playerA.Nickname, 5);
        playerB.Post(10);
        pot.PostBlind(playerB.Nickname, 10);
        playerA.TakeHoleCards("6s2c");
        playerB.TakeHoleCards("6h7h");
        playerC.TakeHoleCards("Ac5c");
        playerC.Post(25);
        pot.PostBet(playerC.Nickname, 25); // Raise 25
        playerA.Fold();
        playerB.Post(15);
        pot.PostBet(playerB.Nickname, 15); // Call 25
        pot.CommitBets();
        table.TakeBoardCards("Ad9d6c6dTs");

        var comboB = new Combo(ComboType.Trips, 20);
        var comboC = new Combo(ComboType.TwoPair, 10);
        evaluator.SetCombo(Game.NoLimitHoldem, table.BoardCards, playerB.HoleCards, comboB);
        evaluator.SetCombo(Game.NoLimitHoldem, table.BoardCards, playerC.HoleCards, comboC);

        // Act
        var events = dealer.Start(
            rules: CreateRules(),
            table: table,
            pot: pot,
            deck: deck,
            randomizer: _randomizer,
            evaluator: evaluator
        ).ToList();

        // Assert
        Assert.Equal(5, events.Count);
        Assert.IsType<StageIsStartedEvent>(events[0]);
        var showEventB = Assert.IsType<HoleCardsAreShownEvent>(events[1]);
        Assert.Equal(playerB.Nickname, showEventB.Nickname);
        Assert.Equal(playerB.HoleCards, showEventB.Cards);
        Assert.Equal(comboB, showEventB.Combo);
        var showEventC = Assert.IsType<HoleCardsAreMuckedEvent>(events[2]);
        Assert.Equal(playerC.Nickname, showEventC.Nickname);
        var winEvent = Assert.IsType<WinIsCommittedEvent>(events[3]);
        Assert.Equal([playerB.Nickname], winEvent.Nicknames);
        Assert.Equal(new Chips(55), winEvent.Amount);
        Assert.IsType<StageIsFinishedEvent>(events[4]);
    }

    [Fact]
    public void Start_PlayersGoAllIn_ShouldShowCards()
    {
        // Arrange
        var dealer = new SettlementDealer();
        var evaluator = new StubEvaluator();
        var pot = CreatePot();
        var playerA = CreatePlayer("Alice", 1, 800);
        var playerB = CreatePlayer("Bobby", 2, 900);
        var playerC = CreatePlayer("Charlie", 6, 1000);
        var table = CreateTable(
            players: [playerA, playerB, playerC],
            smallBlindSeat: 1,
            bigBlindSeat: 2,
            buttonSeat: 6
        );
        var deck = CreateDeck();

        playerA.Post(5);
        pot.PostBlind(playerA.Nickname, 5);
        playerB.Post(10);
        pot.PostBlind(playerB.Nickname, 10);
        playerA.TakeHoleCards("AsKs");
        playerB.TakeHoleCards("JdJh");
        playerC.TakeHoleCards("Ac5c");
        playerC.Post(25);
        pot.PostBet(playerC.Nickname, 25); // Raise 25
        playerA.Post(115);
        pot.PostBet(playerA.Nickname, 115); // Raise 120
        playerB.Post(230);
        pot.PostBet(playerB.Nickname, 230); // Raise 240
        playerC.Post(975);
        pot.PostBet(playerC.Nickname, 975); // Raise 1000 (all-in)
        playerA.Post(680);
        pot.PostBet(playerA.Nickname, 680); // Call 800 (all-in)
        playerB.Post(660);
        pot.PostBet(playerB.Nickname, 660); // Call 900 (all-in)
        pot.RefundBet(playerC.Nickname, 100);
        pot.CommitBets();
        table.TakeBoardCards("Ad9d6c6dTs");

        var comboA = new Combo(ComboType.TwoPair, 30);
        var comboB = new Combo(ComboType.TwoPair, 10);
        var comboC = new Combo(ComboType.TwoPair, 20);
        evaluator.SetCombo(Game.NoLimitHoldem, table.BoardCards, playerA.HoleCards, comboA);
        evaluator.SetCombo(Game.NoLimitHoldem, table.BoardCards, playerB.HoleCards, comboB);
        evaluator.SetCombo(Game.NoLimitHoldem, table.BoardCards, playerC.HoleCards, comboC);

        // Act
        var events = dealer.Start(
            rules: CreateRules(),
            table: table,
            pot: pot,
            deck: deck,
            randomizer: _randomizer,
            evaluator: evaluator
        ).ToList();

        // Assert
        Assert.Equal(7, events.Count);
        Assert.IsType<StageIsStartedEvent>(events[0]);
        var showEventA = Assert.IsType<HoleCardsAreShownEvent>(events[1]);
        Assert.Equal(playerA.Nickname, showEventA.Nickname);
        Assert.Equal(playerA.HoleCards, showEventA.Cards);
        Assert.Equal(comboA, showEventA.Combo);
        var showEventB = Assert.IsType<HoleCardsAreShownEvent>(events[2]);
        Assert.Equal(playerB.Nickname, showEventB.Nickname);
        Assert.Equal(playerB.HoleCards, showEventB.Cards);
        Assert.Equal(comboB, showEventB.Combo);
        var showEventC = Assert.IsType<HoleCardsAreShownEvent>(events[3]);
        Assert.Equal(playerC.Nickname, showEventC.Nickname);
        Assert.Equal(playerC.HoleCards, showEventC.Cards);
        Assert.Equal(comboC, showEventC.Combo);
        var winEvent1 = Assert.IsType<WinIsCommittedEvent>(events[4]);
        Assert.Equal([playerA.Nickname], winEvent1.Nicknames);
        Assert.Equal(new Chips(2400), winEvent1.Amount);
        var winEvent2 = Assert.IsType<WinIsCommittedEvent>(events[5]);
        Assert.Equal([playerC.Nickname], winEvent2.Nicknames);
        Assert.Equal(new Chips(200), winEvent2.Amount);
        Assert.IsType<StageIsFinishedEvent>(events[6]);
    }

    private Pot CreatePot(int minBet = 10)
    {
        return new Pot(minBet: new Chips(minBet));
    }

    private Table CreateTable(List<Player> players, int smallBlindSeat, int bigBlindSeat, int buttonSeat)
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

    private Player CreatePlayer(string nickname, int seat, int stack = 1000)
    {
        return new Player(
            nickname: new Nickname(nickname),
            seat: new Seat(seat),
            stack: new Chips(stack)
        );
    }

    private BaseDeck CreateDeck()
    {
        return new StandardDeck();
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
