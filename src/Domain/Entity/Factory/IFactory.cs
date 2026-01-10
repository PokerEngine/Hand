using Domain.Service.Dealer;
using Domain.ValueObject;

namespace Domain.Entity.Factory;

public interface IFactory
{
    Table GetTable(
        IEnumerable<Participant> participants,
        Seat maxSeat,
        Seat smallBlindSeat,
        Seat bigBlindSeat,
        Seat buttonSeat
    );

    Pot GetPot(Rules rules);

    BaseDeck GetDeck(Rules rules);

    List<IDealer> GetDealers(Rules rules);
}
