using Domain.ValueObject;

namespace Infrastructure.Test.Service.Evaluator;

[Trait("Category", "Integration")]
public class HoldemEvaluatorTest : BaseEvaluatorTest
{
    [Fact]
    public void Evaluate_WhenHighCard_ShouldReturnHighCard()
    {
        // Arrange
        var boardCards = new CardSet([Card.NineOfClubs, Card.SevenOfClubs, Card.SixOfDiamonds, Card.FiveOfHearts, Card.TreyOfDiamonds]);
        var holeCards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        // Act
        var combo = Evaluator.Evaluate(Game.NoLimitHoldem, boardCards, holeCards);

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
        var combo = Evaluator.Evaluate(Game.NoLimitHoldem, boardCards, holeCards);

        // Assert
        Assert.Equal(ComboType.OnePair, combo.Type);
    }

    [Fact]
    public void Evaluate_WhenTwoPair_ShouldReturnTwoPair()
    {
        // Arrange
        var boardCards = new CardSet([Card.AceOfClubs, Card.SevenOfClubs, Card.SixOfDiamonds, Card.FiveOfHearts, Card.DeuceOfDiamonds]);
        var holeCards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        // Act
        var combo = Evaluator.Evaluate(Game.NoLimitHoldem, boardCards, holeCards);

        // Assert
        Assert.Equal(ComboType.TwoPair, combo.Type);
    }

    [Fact]
    public void Evaluate_WhenTrips_ShouldReturnTrips()
    {
        // Arrange
        var boardCards = new CardSet([Card.NineOfClubs, Card.SevenOfClubs, Card.SixOfDiamonds, Card.DeuceOfHearts, Card.DeuceOfDiamonds]);
        var holeCards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        // Act
        var combo = Evaluator.Evaluate(Game.NoLimitHoldem, boardCards, holeCards);

        // Assert
        Assert.Equal(ComboType.Trips, combo.Type);
    }

    [Fact]
    public void Evaluate_WhenStraight_ShouldReturnStraight()
    {
        // Arrange
        var boardCards = new CardSet([Card.NineOfClubs, Card.SevenOfClubs, Card.FiveOfDiamonds, Card.FourOfDiamonds, Card.TreyOfDiamonds]);
        var holeCards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        // Act
        var combo = Evaluator.Evaluate(Game.NoLimitHoldem, boardCards, holeCards);

        // Assert
        Assert.Equal(ComboType.Straight, combo.Type);
    }

    [Fact]
    public void Evaluate_WhenFlush_ShouldReturnFlush()
    {
        // Arrange
        var boardCards = new CardSet([Card.NineOfClubs, Card.SevenOfClubs, Card.FiveOfDiamonds, Card.FourOfDiamonds, Card.TreyOfClubs]);
        var holeCards = new CardSet([Card.AceOfClubs, Card.DeuceOfClubs]);

        // Act
        var combo = Evaluator.Evaluate(Game.NoLimitHoldem, boardCards, holeCards);

        // Assert
        Assert.Equal(ComboType.Flush, combo.Type);
    }

    [Fact]
    public void Evaluate_WhenFullHouse_ShouldReturnFullHouse()
    {
        // Arrange
        var boardCards = new CardSet([Card.AceOfClubs, Card.SevenOfClubs, Card.SixOfDiamonds, Card.DeuceOfHearts, Card.DeuceOfDiamonds]);
        var holeCards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);

        // Act
        var combo = Evaluator.Evaluate(Game.NoLimitHoldem, boardCards, holeCards);

        // Assert
        Assert.Equal(ComboType.FullHouse, combo.Type);
    }

    [Fact]
    public void Evaluate_WhenQuads_ShouldReturnQuads()
    {
        // Arrange
        var boardCards = new CardSet([Card.AceOfClubs, Card.SevenOfClubs, Card.SixOfDiamonds, Card.DeuceOfHearts, Card.DeuceOfDiamonds]);
        var holeCards = new CardSet([Card.DeuceOfSpades, Card.DeuceOfClubs]);

        // Act
        var combo = Evaluator.Evaluate(Game.NoLimitHoldem, boardCards, holeCards);

        // Assert
        Assert.Equal(ComboType.Quads, combo.Type);
    }

    [Fact]
    public void Evaluate_WhenStraightFlush_ShouldStraightFlush()
    {
        // Arrange
        var boardCards = new CardSet([Card.NineOfClubs, Card.SevenOfClubs, Card.FiveOfClubs, Card.FourOfClubs, Card.TreyOfClubs]);
        var holeCards = new CardSet([Card.AceOfClubs, Card.DeuceOfClubs]);

        // Act
        var combo = Evaluator.Evaluate(Game.NoLimitHoldem, boardCards, holeCards);

        // Assert
        Assert.Equal(ComboType.StraightFlush, combo.Type);
    }
}
