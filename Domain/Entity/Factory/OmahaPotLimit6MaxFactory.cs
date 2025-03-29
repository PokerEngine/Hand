using Domain.ValueObject;
using Domain.Service.Dealer;
using Domain.Service.Evaluator;

namespace Domain.Entity.Factory;

public class OmahaPotLimit6MaxFactory : IFactory
{
    public Game GetGame()
    {
        return Game.OmahaPotLimit6Max;
    }

    public BaseTable GetTable(IEnumerable<Participant> participants)
    {
        var players = participants.Select(x => GetPlayer(x));
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

    public Player GetPlayer(Participant participant)
    {
        return new Player(
            nickname: participant.Nickname,
            position: participant.Position,
            stake: participant.Stake
        );
    }
}