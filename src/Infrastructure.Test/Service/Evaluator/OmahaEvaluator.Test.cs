using Domain.ValueObject;

namespace Infrastructure.Test.Service.Evaluator;

[Trait("Category", "Integration")]
public class OmahaEvaluatorTest : BaseEvaluatorTest
{
    [Fact]
    public void TestHighCard()
    {
        var boardCards = new CardSet([Card.NineOfClubs, Card.SevenOfClubs, Card.SixOfDiamonds, Card.FiveOfHearts, Card.TreyOfDiamonds]);
        var holeCards = new CardSet([Card.AceOfHearts, Card.DeuceOfClubs, Card.KingOfSpades, Card.TenOfDiamonds]);

        var combo = Evaluator.Evaluate(Game.PotLimitOmaha, boardCards, holeCards);

        Assert.Equal(ComboType.HighCard, combo.Type);
    }

    [Fact]
    public void TestOnePair()
    {
        var boardCards = new CardSet([Card.NineOfClubs, Card.SevenOfClubs, Card.SixOfDiamonds, Card.FiveOfHearts, Card.TreyOfDiamonds]);
        var holeCards = new CardSet([Card.SevenOfHearts, Card.DeuceOfClubs, Card.KingOfSpades, Card.TenOfDiamonds]);

        var combo = Evaluator.Evaluate(Game.PotLimitOmaha, boardCards, holeCards);

        Assert.Equal(ComboType.OnePair, combo.Type);
    }

    [Fact]
    public void TestTwoPair()
    {
        var boardCards = new CardSet([Card.NineOfClubs, Card.SevenOfClubs, Card.NineOfDiamonds, Card.FiveOfHearts, Card.TreyOfDiamonds]);
        var holeCards = new CardSet([Card.SevenOfHearts, Card.DeuceOfClubs, Card.KingOfSpades, Card.TenOfDiamonds]);

        var combo = Evaluator.Evaluate(Game.PotLimitOmaha, boardCards, holeCards);

        Assert.Equal(ComboType.TwoPair, combo.Type);
    }

    [Fact]
    public void TestFlush()
    {
        var boardCards = new CardSet([Card.TenOfHearts, Card.SevenOfHearts, Card.DeuceOfHearts, Card.FiveOfHearts, Card.TreyOfClubs]);
        var holeCards = new CardSet([Card.KingOfHearts, Card.NineOfHearts, Card.AceOfSpades, Card.TenOfClubs]);

        var combo = Evaluator.Evaluate(Game.PotLimitOmaha, boardCards, holeCards);

        Assert.Equal(ComboType.Flush, combo.Type);
    }

    [Fact]
    public void TestFullHouse()
    {
        var boardCards = new CardSet([Card.NineOfHearts, Card.NineOfDiamonds, Card.FiveOfSpades, Card.FiveOfClubs, Card.TenOfClubs]);
        var holeCards = new CardSet([Card.FiveOfHearts, Card.KingOfSpades, Card.NineOfSpades, Card.QueenOfDiamonds]);

        var combo = Evaluator.Evaluate(Game.PotLimitOmaha, boardCards, holeCards);

        Assert.Equal(ComboType.FullHouse, combo.Type);
    }

    [Fact]
    public void TestStraightFlush()
    {
        var boardCards = new CardSet([Card.SixOfHearts, Card.SevenOfHearts, Card.EightOfHearts, Card.QueenOfSpades, Card.DeuceOfClubs]);
        var holeCards = new CardSet([Card.FiveOfHearts, Card.NineOfHearts, Card.AceOfDiamonds, Card.KingOfClubs]);

        var combo = Evaluator.Evaluate(Game.PotLimitOmaha, boardCards, holeCards);

        Assert.Equal(ComboType.StraightFlush, combo.Type);
    }
}
