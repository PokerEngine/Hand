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
        HandUid uid,
        TableContext tableContext,
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
            HandUid = uid,
            TableContext = tableContext,
            OccurredAt = DateTime.UtcNow
        };
        yield return startEvent;

        var players = GetPlayersForDealing(table);
        if (HasEnoughPlayersForDealing(players))
        {
            foreach (var player in players)
            {
                yield return DealHoleCards(uid, tableContext, player, deck, randomizer);
            }
        }

        var finishEvent = new StageFinishedEvent
        {
            HandUid = uid,
            TableContext = tableContext,
            OccurredAt = DateTime.UtcNow
        };
        yield return finishEvent;
    }

    private List<Player> GetPlayersForDealing(Table table)
    {
        var startSeat = table.Positions.SmallBlindSeat;
        return table.GetPlayersStartingFromSeat(startSeat).Where(x => !x.IsFolded).ToList();
    }

    private bool HasEnoughPlayersForDealing(List<Player> players)
    {
        return players.Count > 1;
    }

    private HoleCardsDealtEvent DealHoleCards(HandUid uid, TableContext tableContext, Player player, BaseDeck deck, IRandomizer randomizer)
    {
        var cards = deck.ExtractRandomCards(count, randomizer);
        player.TakeHoleCards(cards);

        var @event = new HoleCardsDealtEvent
        {
            HandUid = uid,
            TableContext = tableContext,
            Nickname = player.Nickname,
            Cards = cards,
            OccurredAt = DateTime.UtcNow
        };
        return @event;
    }

    public void Handle(
        IEvent @event,
        HandUid uid,
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
        HandUid uid,
        TableContext tableContext,
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
