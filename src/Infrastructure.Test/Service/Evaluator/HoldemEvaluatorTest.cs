using Domain.ValueObject;
using Infrastructure.Service.Evaluator;

namespace Infrastructure.Test.Service.Evaluator;

public class HoldemEvaluatorTest
{
    private readonly PokerStoveEvaluator _evaluator = new();

    [Fact]
    public void TestHighCard()
    {
        var boardCards = new CardSet([Card.NineOfClubs, Card.SevenOfClubs, Card.SixOfDiamonds, Card.FiveOfHearts, Card.TreyOfDiamonds]);
        var holeCards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        var combo = _evaluator.Evaluate(Game.HoldemNoLimit6Max, boardCards, holeCards);

        Assert.Equal(ComboType.HighCard, combo.Type);
    }

    [Fact]
    public void TestOnePair()
    {
        var boardCards = new CardSet([Card.NineOfClubs, Card.SevenOfClubs, Card.SixOfDiamonds, Card.FiveOfHearts, Card.TreyOfDiamonds]);
        var holeCards = new CardSet([Card.SevenOfHearts, Card.DeuceOfClubs, Card.KingOfSpades, Card.TenOfDiamonds]);

        var combo = _evaluator.Evaluate(Game.HoldemNoLimit6Max, boardCards, holeCards);

        Assert.Equal(ComboType.OnePair, combo.Type);
    }

    [Fact]
    public void TestTwoPair()
    {
        var boardCards = new CardSet([Card.AceOfClubs, Card.SevenOfClubs, Card.SixOfDiamonds, Card.FiveOfHearts, Card.DeuceOfDiamonds]);
        var holeCards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        var combo = _evaluator.Evaluate(Game.HoldemNoLimit6Max, boardCards, holeCards);

        Assert.Equal(ComboType.TwoPair, combo.Type);
    }

    [Fact]
    public void TestTrips()
    {
        var boardCards = new CardSet([Card.NineOfClubs, Card.SevenOfClubs, Card.SixOfDiamonds, Card.DeuceOfHearts, Card.DeuceOfDiamonds]);
        var holeCards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        var combo = _evaluator.Evaluate(Game.HoldemNoLimit6Max, boardCards, holeCards);

        Assert.Equal(ComboType.Trips, combo.Type);
    }

    [Fact]
    public void TestStraight()
    {
        var boardCards = new CardSet([Card.NineOfClubs, Card.SevenOfClubs, Card.FiveOfDiamonds, Card.FourOfDiamonds, Card.TreyOfDiamonds]);
        var holeCards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        var combo = _evaluator.Evaluate(Game.HoldemNoLimit6Max, boardCards, holeCards);

        Assert.Equal(ComboType.Straight, combo.Type);
    }

    [Fact]
    public void TestFlush()
    {
        var boardCards = new CardSet([Card.NineOfClubs, Card.SevenOfClubs, Card.FiveOfDiamonds, Card.FourOfDiamonds, Card.TreyOfClubs]);
        var holeCards = new CardSet([Card.AceOfClubs, Card.DeuceOfClubs]);

        var combo = _evaluator.Evaluate(Game.HoldemNoLimit6Max, boardCards, holeCards);

        Assert.Equal(ComboType.Flush, combo.Type);
    }

    [Fact]
    public void TestFullHouse()
    {
        var boardCards = new CardSet([Card.AceOfClubs, Card.SevenOfClubs, Card.SixOfDiamonds, Card.DeuceOfHearts, Card.DeuceOfDiamonds]);
        var holeCards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        var combo = _evaluator.Evaluate(Game.HoldemNoLimit6Max, boardCards, holeCards);

        Assert.Equal(ComboType.FullHouse, combo.Type);
    }

    [Fact]
    public void TestQuads()
    {
        var boardCards = new CardSet([Card.AceOfClubs, Card.SevenOfClubs, Card.SixOfDiamonds, Card.DeuceOfHearts, Card.DeuceOfDiamonds]);
        var holeCards = new CardSet([Card.DeuceOfSpades, Card.DeuceOfClubs]);

        var combo = _evaluator.Evaluate(Game.HoldemNoLimit6Max, boardCards, holeCards);

        Assert.Equal(ComboType.Quads, combo.Type);
    }

    [Fact]
    public void TestStraightFlush()
    {
        var boardCards = new CardSet([Card.NineOfClubs, Card.SevenOfClubs, Card.FiveOfClubs, Card.FourOfClubs, Card.TreyOfClubs]);
        var holeCards = new CardSet([Card.AceOfClubs, Card.DeuceOfClubs]);

        var combo = _evaluator.Evaluate(Game.HoldemNoLimit6Max, boardCards, holeCards);

        Assert.Equal(ComboType.StraightFlush, combo.Type);
    }
}
