using Domain.Service.Randomizer;

namespace Domain.Test.Service.Randomizer;

public class StubRandomizer : IRandomizer
{
    private readonly Dictionary<int, int> _mapping = new();

    public int GetRandomNumber(int maxValue)
    {
        if (!_mapping.TryGetValue(maxValue, out var number))
        {
            return 0;
        }

        return number;
    }

    public void SetRandomNumber(int maxValue, int number)
    {
        _mapping[maxValue] = number;
    }
}
