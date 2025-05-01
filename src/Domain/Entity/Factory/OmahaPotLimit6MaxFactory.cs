using Domain.Service.Dealer;
using Domain.Service.Evaluator;
using Domain.ValueObject;

namespace Domain.Entity.Factory;

public class OmahaPotLimit6MaxFactory : IFactory
{
    public virtual BaseTable GetTable(IEnumerable<Participant> participants)
    {
        var players = participants.Select(GetPlayer);
        return new SixMaxTable(players);
    }

    public BasePot GetPot(Chips smallBlind, Chips bigBlind)
    {
        return new PotLimitPot(smallBlind, bigBlind);
    }

    public BaseDeck GetDeck()
    {
        return new StandardDeck();
    }

    public IEvaluator GetEvaluator()
    {
        return new OmahaEvaluator();
    }

    public IList<IDealer> GetDealers()
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
            position: participant.Position,
            stake: participant.Stake
        );
    }
}
