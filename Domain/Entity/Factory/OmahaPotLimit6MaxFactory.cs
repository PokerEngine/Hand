using Domain.ValueObject;
using Domain.Service.Dealer;
using Domain.Service.Evaluator;

namespace Domain.Entity.Factory;

public class OmahaPotLimit6MaxFactory : IFactory
{
    public Hand Build(
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
            dealerIdx: 0
        );
    }

    public Game GetGame()
    {
        return Game.OmahaPotLimit6Max;
    }

    public BaseTable BuildTable(IEnumerable<Participant> participants)
    {
        var players = participants
            .Select(
                x => new Player(
                    nickname: x.Nickname,
                    position: x.Position,
                    stake: x.Stake,
                    holeCards: new CardSet(),
                    isConnected: false,
                    isFolded: false
                )
            )
            .ToList();
        return new SixMaxTable(
            players: players,
            boardCards: new CardSet()
        );
    }

    public BasePot BuildPot(Chips smallBlind, Chips bigBlind)
    {
        return new PotLimitPot(
            smallBlind: smallBlind,
            bigBlind: bigBlind,
            lastDecisionNickname: null,
            lastRaiseNickname: null,
            lastRaiseStep: bigBlind,
            currentDecisionNicknames: [],
            currentSidePot: new SidePot(),
            previousSidePot: new SidePot()
        );
    }

    public BaseDeck BuildDeck()
    {
        return new StandardDeck(StandardDeck.AllowedCards);
    }

    public IEvaluator BuildEvaluator()
    {
        return new OmahaEvaluator();
    }

    public IList<IDealer> BuildDealers()
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
}