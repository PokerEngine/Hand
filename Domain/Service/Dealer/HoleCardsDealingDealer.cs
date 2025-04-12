using Domain.Entity;
using Domain.Error;
using Domain.Event;
using Domain.ValueObject;
using Domain.Service.Evaluator;

namespace Domain.Service.Dealer;

public class HoleCardsDealingDealer : IDealer
{
    private int _count;

    public HoleCardsDealingDealer(int count)
    {
        _count = count;
    }

    public void Start(
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        IEventBus eventBus
    )
    {
        var startEvent = new StageIsStartedEvent(OccuredAt: DateTime.Now);
        eventBus.Publish(startEvent);

        var players = GetPlayersForDealing(table);
        if (HasEnoughPlayersForDealing(players))
        {
            foreach (var player in players)
            {
                DealHoleCards(
                    player: player,
                    deck: deck,
                    eventBus: eventBus
                );
            }
        }

        var finishEvent = new StageIsFinishedEvent(OccuredAt: DateTime.Now);
        eventBus.Publish(finishEvent);
    }

    private IList<Player> GetPlayersForDealing(BaseTable table)
    {
        return table.Where(x => !x.IsFolded).ToList();
    }

    private bool HasEnoughPlayersForDealing(IList<Player> players)
    {
        return players.Count > 1;
    }

    private void DealHoleCards(
        Player player,
        BaseDeck deck,
        IEventBus eventBus
    )
    {
        var cards = deck.ExtractRandomCards(_count);
        player.TakeHoleCards(cards);

        var @event = new HoleCardsAreDealtEvent(
            Nickname: player.Nickname,
            Cards: cards,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(@event);
    }

    public void Handle(
        IEvent @event,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
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
                throw new NotAvailableError($"The event {@event} is not supported");
        }
    }

    public void CommitDecision(
        Nickname nickname,
        Decision decision,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        IEventBus eventBus
    )
    {
        throw new NotAvailableError("The player cannot commit a decision during this stage");
    }
}