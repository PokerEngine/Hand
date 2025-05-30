using Domain.Service.Randomizer;

namespace Domain.Test.Service.Randomizer;

public class FakeRandomizer : IRandomizer
{
    public int GetRandomNumber(int maxValue)
    {
        return 0;
    }
}
