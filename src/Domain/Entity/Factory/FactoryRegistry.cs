using Domain.ValueObject;

namespace Domain.Entity.Factory;

public static class FactoryRegistry
{
    private static readonly Dictionary<Game, IFactory> Mapping = new()
    {
        { Game.HoldemNoLimit6Max, new HoldemNoLimit6MaxFactory() },
        { Game.HoldemNoLimit9Max, new HoldemNoLimit9MaxFactory() },
        { Game.OmahaPotLimit6Max, new OmahaPotLimit6MaxFactory() },
        { Game.OmahaPotLimit9Max, new OmahaPotLimit9MaxFactory() },
    };

    public static IFactory GetFactory(Game game)
    {
        if (!Mapping.TryGetValue(game, out var factory))
        {
            throw new NotFoundError("The game is not found");
        }

        return factory;
    }
}
