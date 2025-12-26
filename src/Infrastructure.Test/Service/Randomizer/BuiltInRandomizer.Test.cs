using Domain.Service.Randomizer;
using Infrastructure.Service.Randomizer;

namespace Infrastructure.Test.Service.Randomizer;

public class BuiltInRandomizerTest
{
    private readonly IRandomizer _randomizer = CreateRandomizer();

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100500)]
    public void GetRandomNumber_WhenMaxValueSpecified_ShouldReturnFromZeroToMaxValue(int maxValue)
    {
        // Act
        var number = _randomizer.GetRandomNumber(maxValue);

        // Assert
        Assert.True(number >= 0);
        Assert.True(number <= maxValue);
    }

    private static IRandomizer CreateRandomizer()
    {
        return new BuiltInRandomizer();
    }
}
