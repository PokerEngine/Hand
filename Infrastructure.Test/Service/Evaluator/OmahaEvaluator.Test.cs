using Domain.ValueObject;

namespace Infrastructure.Test.Service.Evaluator;

[Trait("Category", "Integration")]
public class OmahaEvaluatorTest : BaseEvaluatorTest
{
    [Fact]
    public void Evaluate_WhenHighCard_ShouldReturnHighCard()
    {
        // Arrange
        var boardCards = new CardSet([Card.NineOfClubs, Card.SevenOfClubs, Card.SixOfDiamonds, Card.FiveOfHearts, Card.TreyOfDiamonds]);
        var holeCards = new CardSet([Card.AceOfHearts, Card.DeuceOfClubs, Card.KingOfSpades, Card.TenOfDiamonds]);

        // Act
        var combo = Evaluator.Evaluate(Game.PotLimitOmaha, boardCards, holeCards);

        // Assert
        Assert.Equal(ComboType.HighCard, combo.Type);
    }

    [Fact]
    public void Evaluate_WhenOnePair_ShouldReturnOnePair()
    {
        // Arrange
        var boardCards = new CardSet([Card.NineOfClubs, Card.SevenOfClubs, Card.SixOfDiamonds, Card.FiveOfHearts, Card.TreyOfDiamonds]);
        var holeCards = new CardSet([Card.SevenOfHearts, Card.DeuceOfClubs, Card.KingOfSpades, Card.TenOfDiamonds]);

        // Act
        var combo = Evaluator.Evaluate(Game.PotLimitOmaha, boardCards, holeCards);

        // Assert
        Assert.Equal(ComboType.OnePair, combo.Type);
    }

    [Fact]
    public void Evaluate_WhenTwoPair_ShouldReturnTwoPair()
    {
        // Arrange
        var boardCards = new CardSet([Card.NineOfClubs, Card.SevenOfClubs, Card.NineOfDiamonds, Card.FiveOfHearts, Card.TreyOfDiamonds]);
        var holeCards = new CardSet([Card.SevenOfHearts, Card.DeuceOfClubs, Card.KingOfSpades, Card.TenOfDiamonds]);

        // Act
        var combo = Evaluator.Evaluate(Game.PotLimitOmaha, boardCards, holeCards);

        // Assert
        Assert.Equal(ComboType.TwoPair, combo.Type);
    }

    [Fact]
    public void Evaluate_WhenTrips_ShouldReturnTrips()
    {
        // Arrange
        var boardCards = new CardSet([Card.NineOfClubs, Card.SevenOfClubs, Card.NineOfDiamonds, Card.FiveOfHearts, Card.TreyOfDiamonds]);
        var holeCards = new CardSet([Card.TenOfDiamonds, Card.DeuceOfClubs, Card.KingOfSpades, Card.NineOfHearts]);

        // Act
        var combo = Evaluator.Evaluate(Game.PotLimitOmaha, boardCards, holeCards);

        // Assert
        Assert.Equal(ComboType.Trips, combo.Type);
    }

    [Fact]
    public void Evaluate_WhenStraight_ShouldReturnStraight()
    {
        // Arrange
        var boardCards = new CardSet([Card.SixOfHearts, Card.SevenOfHearts, Card.EightOfDiamonds, Card.QueenOfSpades, Card.DeuceOfClubs]);
        var holeCards = new CardSet([Card.FiveOfHearts, Card.NineOfHearts, Card.AceOfDiamonds, Card.KingOfClubs]);

        // Act
        var combo = Evaluator.Evaluate(Game.PotLimitOmaha, boardCards, holeCards);

        // Assert
        Assert.Equal(ComboType.Straight, combo.Type);
    }

    [Fact]
    public void Evaluate_WhenFlush_ShouldReturnFlush()
    {
        // Arrange
        var boardCards = new CardSet([Card.TenOfHearts, Card.SevenOfHearts, Card.DeuceOfHearts, Card.FiveOfHearts, Card.TreyOfClubs]);
        var holeCards = new CardSet([Card.KingOfHearts, Card.NineOfHearts, Card.AceOfSpades, Card.TenOfClubs]);

        // Act
        var combo = Evaluator.Evaluate(Game.PotLimitOmaha, boardCards, holeCards);

        // Assert
        Assert.Equal(ComboType.Flush, combo.Type);
    }

    [Fact]
    public void Evaluate_WhenFullHouse_ShouldReturnFullHouse()
    {
        // Arrange
        var boardCards = new CardSet([Card.NineOfHearts, Card.NineOfDiamonds, Card.FiveOfSpades, Card.FiveOfClubs, Card.TenOfClubs]);
        var holeCards = new CardSet([Card.FiveOfHearts, Card.KingOfSpades, Card.NineOfSpades, Card.QueenOfDiamonds]);

        // Act
        var combo = Evaluator.Evaluate(Game.PotLimitOmaha, boardCards, holeCards);

        // Assert
        Assert.Equal(ComboType.FullHouse, combo.Type);
    }

    [Fact]
    public void Evaluate_WhenQuads_ShouldReturnQuads()
    {
        // Arrange
        var boardCards = new CardSet([Card.NineOfHearts, Card.NineOfDiamonds, Card.FiveOfSpades, Card.FiveOfClubs, Card.TenOfClubs]);
        var holeCards = new CardSet([Card.FiveOfHearts, Card.KingOfSpades, Card.NineOfSpades, Card.FiveOfDiamonds]);

        // Act
        var combo = Evaluator.Evaluate(Game.PotLimitOmaha, boardCards, holeCards);

        // Assert
        Assert.Equal(ComboType.Quads, combo.Type);
    }

    [Fact]
    public void Evaluate_WhenStraightFlush_ShouldReturnStraightFlush()
    {
        // Arrange
        var boardCards = new CardSet([Card.SixOfHearts, Card.SevenOfHearts, Card.EightOfHearts, Card.QueenOfSpades, Card.DeuceOfClubs]);
        var holeCards = new CardSet([Card.FiveOfHearts, Card.NineOfHearts, Card.AceOfDiamonds, Card.KingOfClubs]);

        // Act
        var combo = Evaluator.Evaluate(Game.PotLimitOmaha, boardCards, holeCards);

        // Assert
        Assert.Equal(ComboType.StraightFlush, combo.Type);
    }
}
