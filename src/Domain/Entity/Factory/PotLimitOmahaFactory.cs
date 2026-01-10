using Domain.Service.Dealer;
using Domain.ValueObject;

namespace Domain.Entity.Factory;

public class PotLimitOmahaFactory : IFactory
{
    public virtual Table GetTable(
        IEnumerable<Participant> participants,
        Seat maxSeat,
        Seat smallBlindSeat,
        Seat bigBlindSeat,
        Seat buttonSeat
    )
    {
        var players = participants.Select(GetPlayer);
        return new Table(
            players: players,
            maxSeat: maxSeat,
            smallBlindSeat: smallBlindSeat,
            bigBlindSeat: bigBlindSeat,
            buttonSeat: buttonSeat
        );
    }

    public Pot GetPot(Rules rules)
    {
        return new Pot(minBet: rules.BigBlind);
    }

    public BaseDeck GetDeck(Rules rules)
    {
        return new StandardDeck();
    }

    public List<IDealer> GetDealers(Rules rules)
    {
        return [
            new BlindPostingDealer(),
            new HoleCardsDealingDealer(4),
            new PotLimitBettingDealer(),
            new BoardCardsDealingDealer(3),
            new PotLimitBettingDealer(),
            new BoardCardsDealingDealer(1),
            new PotLimitBettingDealer(),
            new BoardCardsDealingDealer(1),
            new PotLimitBettingDealer(),
            new SettlementDealer(),
        ];
    }

    private Player GetPlayer(Participant participant)
    {
        return new Player(
            nickname: participant.Nickname,
            seat: participant.Seat,
            stack: participant.Stack
        );
    }
}
