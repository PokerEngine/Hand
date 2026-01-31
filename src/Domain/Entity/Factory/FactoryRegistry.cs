using Domain.Exception;
using Domain.ValueObject;

namespace Domain.Entity.Factory;

public static class FactoryRegistry
{
    private static readonly Dictionary<Game, IFactory> Mapping = new()
    {
        { Game.NoLimitHoldem, new NoLimitHoldemFactory() },
        { Game.PotLimitOmaha, new PotLimitOmahaFactory() },
    };

    public static IFactory GetFactory(Game game)
    {
        if (!Mapping.TryGetValue(game, out var factory))
        {
            throw new InvalidHandConfigurationException($"The game is not supported: {game}");
        }

        return factory;
    }
}
