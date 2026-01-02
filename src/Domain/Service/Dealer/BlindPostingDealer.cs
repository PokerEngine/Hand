using Domain.Entity;
using Domain.Event;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Domain.Service.Dealer;

public class BlindPostingDealer : IDealer
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

        var smallBlindPostedEvent = PostSmallBlind(table, pot);
        if (smallBlindPostedEvent is not null)
        {
            yield return smallBlindPostedEvent;
        }

        var bigBlindPostedEvent = PostBigBlind(table, pot);
        if (bigBlindPostedEvent is not null)
        {
            yield return bigBlindPostedEvent;
        }

        var finishEvent = new StageIsFinishedEvent
        {
            OccurredAt = DateTime.Now
        };
        yield return finishEvent;
    }

    private SmallBlindIsPostedEvent? PostSmallBlind(Table table, BasePot pot)
    {
        var player = table.GetPlayerOnSmallBlind();
        if (player is null)
        {
            return null;
        }

        var amount = player.Stack < pot.SmallBlind ? player.Stack : pot.SmallBlind;
        pot.PostSmallBlind(player, amount);

        var @event = new SmallBlindIsPostedEvent
        {
            Nickname = player.Nickname,
            Amount = amount,
            OccurredAt = DateTime.Now
        };
        return @event;
    }

    private BigBlindIsPostedEvent? PostBigBlind(Table table, BasePot pot)
    {
        var player = table.GetPlayerOnBigBlind();
        if (player is null)
        {
            return null;
        }

        var amount = player.Stack < pot.BigBlind ? player.Stack : pot.BigBlind;
        pot.PostBigBlind(player, amount);

        var @event = new BigBlindIsPostedEvent
        {
            Nickname = player.Nickname,
            Amount = amount,
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
