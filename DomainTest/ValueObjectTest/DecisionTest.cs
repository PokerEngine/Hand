using Domain.ValueObject;

namespace DomainTest.ValueObjectTest;

public class DecisionTest
{
    [Theory]
    [InlineData(DecisionType.Fold, 0)]
    [InlineData(DecisionType.Check, 0)]
    [InlineData(DecisionType.CallTo, 10)]
    [InlineData(DecisionType.RaiseTo, 10)]
    public void TestInitialization(DecisionType type, int amount)
    {
        var decision = new Decision(type, new Chips(amount));
        Assert.Equal(type, decision.Type);
        Assert.Equal(new Chips(amount), decision.Amount);
    }

    [Theory]
    [InlineData(DecisionType.Fold, 10)]
    [InlineData(DecisionType.Check, 10)]
    public void TestInitializationWithNonZeroAmount(DecisionType type, int amount)
    {
        var exc = Assert.Throws<ArgumentException>(() => new Decision(type, new Chips(amount)));
        Assert.Equal($"Amount must be zero for {type}", exc.Message);
    }

    [Theory]
    [InlineData(DecisionType.CallTo, 0)]
    [InlineData(DecisionType.RaiseTo, 0)]
    public void TestInitializationWithZeroAmount(DecisionType type, int amount)
    {
        var exc = Assert.Throws<ArgumentException>(() => new Decision(type, new Chips(amount)));
        Assert.Equal($"Amount must be non-zero for {type}", exc.Message);
    }

    [Fact]
    public void TestRepresentationWithZeroAmount()
    {
        var decision = new Decision(DecisionType.Check, new Chips(0));

        Assert.Equal("Check", $"{decision}");
    }

    [Fact]
    public void TestRepresentationWithNonZeroAmount()
    {
        var decision = new Decision(DecisionType.RaiseTo, new Chips(10));

        Assert.Equal("RaiseTo [10 chip(s)]", $"{decision}");
    }
}