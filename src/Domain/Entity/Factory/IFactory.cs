using Domain.Service.Dealer;
using Domain.ValueObject;

namespace Domain.Entity.Factory;

public interface IFactory
{
    BaseTable GetTable(
        IEnumerable<Participant> participants,
        Seat smallBlindSeat,
        Seat bigBlindSeat,
        Seat buttonSeat
    );

    BasePot GetPot(Chips smallBlind, Chips bigBlind);

    BaseDeck GetDeck();

    IList<IDealer> GetDealers();
}
