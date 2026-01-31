using Domain.Service.Randomizer;

namespace Application.Test.Service.Randomizer;

public class StubRandomizer : IRandomizer
{
    private static readonly Random Random = new();

    public int GetRandomNumber(int maxValue)
    {
        // `Random` returns a number which is less than `maxValue` but we want less or equal one
        return Random.Next(maxValue + 1);
    }
}
