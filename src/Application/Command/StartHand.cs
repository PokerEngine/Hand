using Application.Event;
using Application.Repository;
using Domain.Entity;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Application.Command;

public record StartHandCommand : ICommand
{
    public required Guid TableUid { get; init; }
    public required string TableType { get; init; }
    public required string Game { get; init; }
    public required int SmallBlind { get; init; }
    public required int BigBlind { get; init; }
    public required int MaxSeat { get; init; }
    public required int SmallBlindSeat { get; init; }
    public required int BigBlindSeat { get; init; }
    public required int ButtonSeat { get; init; }
    public required List<StartHandCommandParticipant> Participants { get; init; }
}

public record StartHandCommandParticipant
{
    public required string Nickname { get; init; }
    public required int Seat { get; init; }
    public required int Stack { get; init; }
}

public record StartHandResponse : ICommandResponse
{
    public required Guid Uid { get; init; }
}

public class StartHandHandler(
    IRepository repository,
    IEventDispatcher eventDispatcher,
    IRandomizer randomizer,
    IEvaluator evaluator
) : ICommandHandler<StartHandCommand, StartHandResponse>
{
    public async Task<StartHandResponse> HandleAsync(StartHandCommand command)
    {
        var tableType = (TableType)Enum.Parse(typeof(TableType), command.TableType);
        var game = (Game)Enum.Parse(typeof(Game), command.Game);

        var rules = new Rules
        {
            Game = game,
            SmallBlind = command.SmallBlind,
            BigBlind = command.BigBlind
        };
        var positions = new Positions
        {
            Max = command.MaxSeat,
            SmallBlind = command.SmallBlindSeat,
            BigBlind = command.BigBlindSeat,
            Button = command.ButtonSeat
        };

        var hand = Hand.FromScratch(
            uid: await repository.GetNextUidAsync(),
            tableUid: command.TableUid,
            tableType: tableType,
            rules: rules,
            positions: positions,
            participants: command.Participants.Select(DeserializeParticipant).ToList(),
            randomizer: randomizer,
            evaluator: evaluator
        );
        hand.Start();

        var events = hand.PullEvents();
        await repository.AddEventsAsync(hand.Uid, events);

        var context = new EventContext
        {
            HandUid = hand.Uid,
            TableUid = hand.TableUid,
            TableType = hand.TableType
        };

        foreach (var @event in events)
        {
            await eventDispatcher.DispatchAsync(@event, context);
        }

        return new StartHandResponse
        {
            Uid = hand.Uid
        };
    }

    private Participant DeserializeParticipant(StartHandCommandParticipant participant)
    {
        return new Participant
        {
            Nickname = participant.Nickname,
            Seat = participant.Seat,
            Stack = participant.Stack
        };
    }
}
