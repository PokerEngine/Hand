using Domain.Entity;
using Domain.Event;
using Domain.Exception;
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
        var startEvent = new StageStartedEvent
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

        var finishEvent = new StageFinishedEvent
        {
            OccurredAt = DateTime.Now
        };
        yield return finishEvent;
    }

    private SmallBlindPostedEvent? PostSmallBlind(Rules rules, Table table, Pot pot)
    {
        var player = table.GetPlayerOnSmallBlind();
        if (player is null)
        {
            return null;
        }

        var amount = player.Stack < rules.SmallBlind ? player.Stack : rules.SmallBlind;
        player.Post(amount);
        pot.PostBlind(player.Nickname, amount);

        var @event = new SmallBlindPostedEvent
        {
            Nickname = player.Nickname,
            Amount = amount,
            OccurredAt = DateTime.Now
        };
        return @event;
    }

    private BigBlindPostedEvent? PostBigBlind(Rules rules, Table table, Pot pot)
    {
        var player = table.GetPlayerOnBigBlind();
        if (player is null)
        {
            return null;
        }

        var amount = player.Stack < rules.BigBlind ? player.Stack : rules.BigBlind;
        player.Post(amount);
        pot.PostBlind(player.Nickname, amount);

        var @event = new BigBlindPostedEvent
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
            case SmallBlindPostedEvent e:
                table.GetPlayerByNickname(e.Nickname).Post(e.Amount);
                pot.PostBlind(e.Nickname, e.Amount);
                break;
            case BigBlindPostedEvent e:
                table.GetPlayerByNickname(e.Nickname).Post(e.Amount);
                pot.PostBlind(e.Nickname, e.Amount);
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
