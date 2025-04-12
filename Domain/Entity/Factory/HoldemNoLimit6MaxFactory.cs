using Domain.ValueObject;
using Domain.Service.Dealer;
using Domain.Service.Evaluator;

namespace Domain.Entity.Factory;

public class HoldemNoLimit6MaxFactory : IFactory
{
    public virtual BaseTable GetTable(IEnumerable<Participant> participants)
    {
        var players = participants.Select(GetPlayer);
        return new SixMaxTable(players);
    }

    public BasePot GetPot(Chips smallBlind, Chips bigBlind)
    {
        return new NoLimitPot(smallBlind, bigBlind);
    }

    public BaseDeck GetDeck()
    {
        return new StandardDeck();
    }

    public IEvaluator GetEvaluator()
    {
        return new HoldemEvaluator();
    }

    public IList<IDealer> GetDealers()
    {
        return [
            new BlindPostingDealer(),
            new HoleCardsDealingDealer(2),
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
            position: participant.Position,
            stake: participant.Stake
        );
    }
}