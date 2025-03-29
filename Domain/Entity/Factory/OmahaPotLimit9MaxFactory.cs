using Domain.ValueObject;

namespace Domain.Entity.Factory;

public class OmahaPotLimit9MaxFactory : OmahaPotLimit6MaxFactory
{
    public new Game GetGame()
    {
        return Game.OmahaPotLimit9Max;
    }

    public new BaseTable GetTable(IEnumerable<Participant> participants)
    {
        var players = participants.Select(x => GetPlayer(x));
        return new NineMaxTable(players);
    }
}