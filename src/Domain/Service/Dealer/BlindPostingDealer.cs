using Domain.Entity;
using Domain.Event;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Domain.Service.Dealer;

public class BlindPostingDealer : IDealer
{
    public void Start(
        Game game,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator,
        IEventBus eventBus
    )
    {
        var startEvent = new StageIsStartedEvent(OccuredAt: DateTime.Now);
        eventBus.Publish(startEvent);

        PostSmallBlind(table: table, pot: pot, eventBus: eventBus);
        PostBigBlind(table: table, pot: pot, eventBus: eventBus);

        var finishEvent = new StageIsFinishedEvent(OccuredAt: DateTime.Now);
        eventBus.Publish(finishEvent);
    }

    private void PostSmallBlind(
        BaseTable table,
        BasePot pot,
        IEventBus eventBus
    )
    {
        var player = table.GetPlayerOnSmallBlind();
        if (player == null)
        {
            return;
        }

        var amount = player.Stake < pot.SmallBlind ? player.Stake : pot.SmallBlind;
        pot.PostSmallBlind(player, amount);

        var @event = new SmallBlindIsPostedEvent(
            Nickname: player.Nickname,
            Amount: amount,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(@event);
    }

    private void PostBigBlind(
        BaseTable table,
        BasePot pot,
        IEventBus eventBus
    )
    {
        var player = table.GetPlayerOnSmallBlind();
        if (player == null)
        {
            return;
        }

        var amount = player.Stake < pot.BigBlind ? player.Stake : pot.BigBlind;
        pot.PostBigBlind(player, amount);

        var @event = new BigBlindIsPostedEvent(
            Nickname: player.Nickname,
            Amount: amount,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(@event);
    }

    public void Handle(
        BaseEvent @event,
        Game game,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        switch (@event)
        {
            case SmallBlindIsPostedEvent e:
                pot.PostSmallBlind(table.GetPlayerByNickname(e.Nickname), e.Amount);
                break;
            case BigBlindIsPostedEvent e:
                pot.PostBigBlind(table.GetPlayerByNickname(e.Nickname), e.Amount);
                break;
            case StageIsStartedEvent:
                break;
            case StageIsFinishedEvent:
                break;
            default:
                throw new ArgumentException("The event is not supported", nameof(@event));
        }
    }

    public void CommitDecision(
        Nickname nickname,
        Decision decision,
        Game game,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator,
        IEventBus eventBus
    )
    {
        throw new InvalidOperationException("The player cannot commit a decision during this stage");
    }
}
