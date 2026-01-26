using Domain.ValueObject;

namespace Domain.Test.ValueObject;

public class PlayerActionTest
{
    [Theory]
    [InlineData(PlayerActionType.Fold, 0)]
    [InlineData(PlayerActionType.Check, 0)]
    [InlineData(PlayerActionType.CallBy, 10)]
    [InlineData(PlayerActionType.RaiseBy, 10)]
    public void TestInitialization(PlayerActionType type, int amount)
    {
        var action = new PlayerAction(type, new Chips(amount));
        Assert.Equal(type, action.Type);
        Assert.Equal(new Chips(amount), action.Amount);
    }

    [Theory]
    [InlineData(PlayerActionType.Fold, 10)]
    [InlineData(PlayerActionType.Check, 10)]
    public void TestInitializationWithNonZeroAmount(PlayerActionType type, int amount)
    {
        var exc = Assert.Throws<ArgumentException>(() => new PlayerAction(type, new Chips(amount)));
        Assert.StartsWith($"Amount must be zero for {type}", exc.Message);
    }

    [Theory]
    [InlineData(PlayerActionType.RaiseBy, 0)]
    [InlineData(PlayerActionType.CallBy, 0)]
    public void TestInitializationWithZeroAmount(PlayerActionType type, int amount)
    {
        var exc = Assert.Throws<ArgumentException>(() => new PlayerAction(type, new Chips(amount)));
        Assert.StartsWith($"Amount must be non-zero for {type}", exc.Message);
    }

    [Fact]
    public void TestRepresentationWithZeroAmount()
    {
        var action = new PlayerAction(PlayerActionType.Check, new Chips(0));

        Assert.Equal("Check", $"{action}");
    }

    [Fact]
    public void TestRepresentationWithNonZeroAmount()
    {
        var action = new PlayerAction(PlayerActionType.RaiseBy, new Chips(10));

        Assert.Equal("RaiseBy [10 chip(s)]", $"{action}");
    }
}
