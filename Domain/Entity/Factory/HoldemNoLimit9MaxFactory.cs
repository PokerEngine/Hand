using Domain.ValueObject;

namespace Domain.Entity.Factory;

public class HoldemNoLimit9MaxFactory : HoldemNoLimit6MaxFactory
{
    public new Game GetGame()
    {
        return Game.HoldemNoLimit9Max;
    }

    public new BaseTable BuildTable(IEnumerable<Participant> participants)
    {
        var players = participants
            .Select(
                x => new Player(
                    nickname: x.Nickname,
                    position: x.Position,
                    stake: x.Stake,
                    holeCards: new CardSet(),
                    isConnected: false,
                    isFolded: false
                )
            )
            .ToList();
        return new NineMaxTable(
            players: players,
            boardCards: new CardSet()
        );
    }
}