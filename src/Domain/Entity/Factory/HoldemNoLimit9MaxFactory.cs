using Domain.ValueObject;

namespace Domain.Entity.Factory;

public class HoldemNoLimit9MaxFactory : HoldemNoLimit6MaxFactory
{
    public override BaseTable GetTable(IEnumerable<Participant> participants)
    {
        var players = participants.Select(GetPlayer);
        return new NineMaxTable(players);
    }
}
