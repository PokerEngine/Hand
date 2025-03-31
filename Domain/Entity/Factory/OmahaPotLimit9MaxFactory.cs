using Domain.ValueObject;

namespace Domain.Entity.Factory;

public class OmahaPotLimit9MaxFactory : OmahaPotLimit6MaxFactory
{
    public override BaseTable GetTable(IEnumerable<Participant> participants)
    {
        var players = participants.Select(x => GetPlayer(x));
        return new NineMaxTable(players);
    }
}