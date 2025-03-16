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
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        var players = GetPlayersForDealing(table);
        if (HasEnoughPlayersForDealing(players))
        {
            var startEvent = new StageIsStartedEvent(HandUid: handUid, OccuredAt: DateTime.Now);
            eventBus.Publish(startEvent);

            foreach (var player in players)
            {
                DealHoleCards(
                    player: player,
                    deck: deck,
                    handUid: handUid,
                    eventBus: eventBus
                );
            }

            var finishEvent = new StageIsFinishedEvent(HandUid: handUid, OccuredAt: DateTime.Now);
            eventBus.Publish(finishEvent);
        }
        else
        {
            var skipEvent = new StageIsSkippedEvent(HandUid: handUid, OccuredAt: DateTime.Now);
            eventBus.Publish(skipEvent);
        }
    }

    private IList<Player> GetPlayersForDealing(BaseTable table)
    {
        return table.Players.Where(x => x.IsAvailableForDealing).ToList();
    }

    private bool HasEnoughPlayersForDealing(IList<Player> players)
    {
        return players.Count > 1;
    }

    private void DealHoleCards(
        Player player,
        BaseDeck deck,
        HandUid handUid,
        EventBus eventBus
    )
    {
        var cards = deck.Extract(_count);
        player.TakeHoleCards(cards);

        var @event = new HoleCardsAreDealtEvent(
            Nickname: player.Nickname,
            Cards: cards,
            HandUid: handUid,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(@event);
    }

    public void Fold(
        Nickname nickname,
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        throw new NotAvailableError("The player cannot fold during this stage");
    }

    public void Check(
        Nickname nickname,
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        throw new NotAvailableError("The player cannot check during this stage");
    }

    public void CallTo(
        Nickname nickname,
        Chips amount,
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        throw new NotAvailableError("The player cannot call during this stage");
    }

    public void RaiseTo(
        Nickname nickname,
        Chips amount,
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        throw new NotAvailableError("The player cannot raise during this stage");
    }
}