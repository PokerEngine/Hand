using Domain.Entity;
using Domain.Event;
using Domain.Exception;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Domain.Service.Dealer;

public class HoleCardsDealingDealer(int count) : IDealer
{
    public IEnumerable<IEvent> Start(
        Rules rules,
        Table table,
        Pot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        var startEvent = new StageStartedEvent
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

        var finishEvent = new StageFinishedEvent
        {
            OccurredAt = DateTime.Now
        };
        yield return finishEvent;
    }

    private List<Player> GetPlayersForDealing(Table table)
    {
        var startSeat = table.Positions.SmallBlind;
        return table.GetPlayersStartingFromSeat(startSeat).Where(x => !x.IsFolded).ToList();
    }

    private bool HasEnoughPlayersForDealing(List<Player> players)
    {
        return players.Count > 1;
    }

    private HoleCardsDealtEvent DealHoleCards(Player player, BaseDeck deck, IRandomizer randomizer)
    {
        var cards = deck.ExtractRandomCards(count, randomizer);
        player.TakeHoleCards(cards);

        var @event = new HoleCardsDealtEvent
        {
            Nickname = player.Nickname,
            Cards = cards,
            OccurredAt = DateTime.Now
        };
        return @event;
    }

    public void Handle(
        IEvent @event,
        Rules rules,
        Table table,
        Pot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        switch (@event)
        {
            case HoleCardsDealtEvent e:
                table.GetPlayerByNickname(e.Nickname).TakeHoleCards(deck.ExtractCertainCards(e.Cards));
                break;
            case StageStartedEvent:
                break;
            case StageFinishedEvent:
                break;
            default:
                throw new InvalidHandStateException($"{@event.GetType().Name} is not supported");
        }
    }

    public IEnumerable<IEvent> SubmitPlayerAction(
        Nickname nickname,
        PlayerAction action,
        Rules rules,
        Table table,
        Pot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        throw new PlayerActionNotAllowedException("The player cannot act during this stage");
    }
}
