using Domain.ValueObject;

namespace Domain.Entity.Factory;

public class OmahaPotLimit9MaxFactory : OmahaPotLimit6MaxFactory
{
    public override BaseTable GetTable(
        IEnumerable<Participant> participants,
        Seat smallBlindSeat,
        Seat bigBlindSeat,
        Seat buttonSeat
    )
    {
        var players = participants.Select(GetPlayer);
        return new NineMaxTable(
            players: players,
            smallBlindSeat: smallBlindSeat,
            bigBlindSeat: bigBlindSeat,
            buttonSeat: buttonSeat
        );
    }
}
