using Domain.ValueObject;
using Domain.Service.Dealer;
using Domain.Service.Evaluator;

namespace Domain.Entity.Factory;

public interface IFactory
{
    public Game GetGame();

    public BaseTable GetTable(IEnumerable<Participant> participants);

    public BasePot GetPot(Chips smallBlind, Chips bigBlind);

    public BaseDeck GetDeck();

    public IEvaluator GetEvaluator();

    public IList<IDealer> GetDealers();

    public Player GetPlayer(Participant participant);
}