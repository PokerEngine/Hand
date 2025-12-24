using Domain.Service.Dealer;
using Domain.ValueObject;

namespace Domain.Entity.Factory;

public class OmahaPotLimitFactory : IFactory
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

    public BasePot GetPot(Chips smallBlind, Chips bigBlind)
    {
        return new PotLimitPot(smallBlind, bigBlind);
    }

    public BaseDeck GetDeck()
    {
        return new StandardDeck();
    }

    public List<IDealer> GetDealers()
    {
        return [
            new BlindPostingDealer(),
            new HoleCardsDealingDealer(4),
            new TradingDealer(),
            new BoardCardsDealingDealer(3),
            new TradingDealer(),
            new BoardCardsDealingDealer(1),
            new TradingDealer(),
            new BoardCardsDealingDealer(1),
            new TradingDealer(),
            new ShowdownDealer(),
        ];
    }

    protected Player GetPlayer(Participant participant)
    {
        return new Player(
            nickname: participant.Nickname,
            seat: participant.Seat,
            stack: participant.Stack
        );
    }
}
