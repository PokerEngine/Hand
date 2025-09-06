using Domain.ValueObject;

namespace Domain.Entity.Factory;

public static class FactoryRegistry
{
    private static readonly Dictionary<Game, IFactory> Mapping = new()
    {
        { Game.HoldemNoLimit, new HoldemNoLimitFactory() },
        { Game.OmahaPotLimit, new OmahaPotLimitFactory() },
    };

    public static IFactory GetFactory(Game game)
    {
        if (!Mapping.TryGetValue(game, out var factory))
        {
            throw new ArgumentException("The game is not found", nameof(game));
        }

        return factory;
    }
}
