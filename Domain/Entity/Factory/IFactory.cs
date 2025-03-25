using Domain.Entity;
using Domain.ValueObject;
using Domain.Service.Dealer;
using Domain.Service.Evaluator;

namespace Domain.Entity.Factory;

public interface IFactory
{
    public Hand Build(
        HandUid handUid,
        Chips smallBlind,
        Chips bigBlind,
        IEnumerable<Participant> participants
    );

    public Game GetGame();

    public BaseTable BuildTable(IEnumerable<Participant> participants);

    public BasePot BuildPot(Chips smallBlind, Chips bigBlind);

    public BaseDeck BuildDeck();

    public IEvaluator BuildEvaluator();

    public IList<IDealer> BuildDealers();

    private Hand Build1111(
        HandUid handUid,
        Chips smallBlind,
        Chips bigBlind,
        IEnumerable<Participant> participants
    )
    {
        return new Hand(
            uid: handUid,
            game: GetGame(),
            table: BuildTable(participants),
            pot: BuildPot(smallBlind, bigBlind),
            deck: BuildDeck(),
            evaluator: BuildEvaluator(),
            dealers: BuildDealers(),
            dealerIdx: -1
        );
    }
}