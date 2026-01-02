using Domain.Entity;
using Domain.Event;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Domain.Service.Dealer;

public class HoleCardsDealingDealer(int count) : IDealer
{
    public IEnumerable<IEvent> Start(
        Game game,
        Table table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        var startEvent = new StageIsStartedEvent
        {
            OccurredAt = DateTime.Now
        };
        yield return startEvent;

        var players = GetPlayersForDealing(table);
        if (HasEnoughPlayersForDealing(players))
        {
            foreach (var player in players)
            {
                yield return DealHoleCards(player, deck, randomizer);
            }
        }

        var finishEvent = new StageIsFinishedEvent
        {
            OccurredAt = DateTime.Now
        };
        yield return finishEvent;
    }

    private List<Player> GetPlayersForDealing(Table table)
    {
        var startSeat = table.SmallBlindSeat;
        return table.GetPlayersStartingFromSeat(startSeat).Where(x => !x.IsFolded).ToList();
    }

    private bool HasEnoughPlayersForDealing(List<Player> players)
    {
        return players.Count > 1;
    }

    private HoleCardsAreDealtEvent DealHoleCards(Player player, BaseDeck deck, IRandomizer randomizer)
    {
        var cards = deck.ExtractRandomCards(count, randomizer);
        player.TakeHoleCards(cards);

        var @event = new HoleCardsAreDealtEvent
        {
            Nickname = player.Nickname,
            Cards = player.HoleCards,
            OccurredAt = DateTime.Now
        };
        return @event;
    }

    public void Handle(
        IEvent @event,
        Game game,
        Table table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        switch (@event)
        {
            case HoleCardsAreDealtEvent e:
                table.GetPlayerByNickname(e.Nickname).TakeHoleCards(deck.ExtractCertainCards(e.Cards));
                break;
            case StageIsStartedEvent:
                break;
            case StageIsFinishedEvent:
                break;
            default:
                throw new ArgumentException("The event is not supported", nameof(@event));
        }
    }

    public IEnumerable<IEvent> CommitDecision(
        Nickname nickname,
        Decision decision,
        Game game,
        Table table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        throw new InvalidOperationException("The player cannot commit a decision during this stage");
    }
}
