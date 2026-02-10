using Domain.Service.Dealer;
using Domain.ValueObject;

namespace Domain.Entity.Factory;

public class NoLimitHoldemFactory : IFactory
{
    public virtual Table GetTable(
        IEnumerable<Participant> players,
        Rules rules,
        Positions positions
    )
    {
        return new Table(
            players: players.Select(GetPlayer),
            rules: rules,
            positions: positions
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
            new HoleCardsDealingDealer(2),
            new NoLimitBettingDealer(),
            new BoardCardsDealingDealer(3),
            new NoLimitBettingDealer(),
            new BoardCardsDealingDealer(1),
            new NoLimitBettingDealer(),
            new BoardCardsDealingDealer(1),
            new NoLimitBettingDealer(),
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
