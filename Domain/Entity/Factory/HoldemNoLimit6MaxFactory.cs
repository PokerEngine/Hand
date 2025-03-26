using Domain.ValueObject;
using Domain.Service.Dealer;
using Domain.Service.Evaluator;

namespace Domain.Entity.Factory;

public class HoldemNoLimit6MaxFactory : IFactory
{
    public Game GetGame()
    {
        return Game.HoldemNoLimit6Max;
    }

    public BaseTable GetTable(IEnumerable<Participant> participants)
    {
        var players = participants.Select(x => GetPlayer(x));
        return new SixMaxTable(
            players: players,
            boardCards: new CardSet()
        );
    }

    public BasePot GetPot(Chips smallBlind, Chips bigBlind)
    {
        return new NoLimitPot(
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

    public BaseDeck GetDeck()
    {
        return new StandardDeck(StandardDeck.AllowedCards);
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

    public Player GetPlayer(Participant participant)
    {
        return new Player(
            nickname: participant.Nickname,
            position: participant.Position,
            stake: participant.Stake,
            holeCards: new CardSet(),
            isConnected: false,
            isFolded: false
        );
    }
}