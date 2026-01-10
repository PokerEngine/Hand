using Domain.Entity;
using Domain.Event;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Domain.Service.Dealer;

public class BlindPostingDealer : IDealer
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
        var startEvent = new StageIsStartedEvent
        {
            OccurredAt = DateTime.Now
        };
        yield return startEvent;

        var smallBlindPostedEvent = PostSmallBlind(rules, table, pot);
        if (smallBlindPostedEvent is not null)
        {
            yield return smallBlindPostedEvent;
        }

        var bigBlindPostedEvent = PostBigBlind(rules, table, pot);
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

    private SmallBlindIsPostedEvent? PostSmallBlind(Rules rules, Table table, Pot pot)
    {
        var player = table.GetPlayerOnSmallBlind();
        if (player is null)
        {
            return null;
        }

        var amount = player.Stack < rules.SmallBlind ? player.Stack : rules.SmallBlind;
        player.Post(amount);
        pot.PostBlind(player.Nickname, amount);

        var @event = new SmallBlindIsPostedEvent
        {
            Nickname = player.Nickname,
            Amount = amount,
            OccurredAt = DateTime.Now
        };
        return @event;
    }

    private BigBlindIsPostedEvent? PostBigBlind(Rules rules, Table table, Pot pot)
    {
        var player = table.GetPlayerOnBigBlind();
        if (player is null)
        {
            return null;
        }

        var amount = player.Stack < rules.BigBlind ? player.Stack : rules.BigBlind;
        player.Post(amount);
        pot.PostBlind(player.Nickname, amount);

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
            case SmallBlindIsPostedEvent e:
                table.GetPlayerByNickname(e.Nickname).Post(e.Amount);
                pot.PostBlind(e.Nickname, e.Amount);
                break;
            case BigBlindIsPostedEvent e:
                table.GetPlayerByNickname(e.Nickname).Post(e.Amount);
                pot.PostBlind(e.Nickname, e.Amount);
                break;
            case StageIsStartedEvent:
                break;
            case StageIsFinishedEvent:
                break;
            default:
                throw new InvalidOperationException($"{@event.GetType().Name} is not supported");
        }
    }

    public IEnumerable<IEvent> CommitDecision(
        Nickname nickname,
        Decision decision,
        Rules rules,
        Table table,
        Pot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        throw new InvalidOperationException("The player cannot commit a decision during this stage");
    }
}
