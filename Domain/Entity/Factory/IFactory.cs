using Domain.Service.Dealer;
using Domain.Service.Evaluator;
using Domain.ValueObject;

namespace Domain.Entity.Factory;

public interface IFactory
{
    public BaseTable GetTable(IEnumerable<Participant> participants);

    public BasePot GetPot(Chips smallBlind, Chips bigBlind);

    public BaseDeck GetDeck();

    public IEvaluator GetEvaluator();

    public IList<IDealer> GetDealers();
}
