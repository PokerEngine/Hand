using Domain.Service.Dealer;
using Domain.ValueObject;

namespace Domain.Entity.Factory;

public interface IFactory
{
    public BaseTable GetTable(IEnumerable<Participant> participants);

    public BasePot GetPot(Chips smallBlind, Chips bigBlind);

    public BaseDeck GetDeck();

    public IList<IDealer> GetDealers();
}
