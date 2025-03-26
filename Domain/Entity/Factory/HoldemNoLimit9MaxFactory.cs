using Domain.ValueObject;

namespace Domain.Entity.Factory;

public class HoldemNoLimit9MaxFactory : HoldemNoLimit6MaxFactory
{
    public new Game GetGame()
    {
        return Game.HoldemNoLimit9Max;
    }

    public new BaseTable GetTable(IEnumerable<Participant> participants)
    {
        var players = participants.Select(x => GetPlayer(x));
        return new NineMaxTable(
            players: players,
            boardCards: new CardSet()
        );
    }
}