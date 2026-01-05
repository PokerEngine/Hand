using Domain.Service.Randomizer;

namespace Domain.Test.Service.Randomizer;

public class StubRandomizer : IRandomizer
{
    public int GetRandomNumber(int maxValue)
    {
        return 0;
    }
}
