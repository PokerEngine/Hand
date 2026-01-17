using Application.Event;
using Application.Repository;
using Domain.Entity;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Application.Command;

public record struct CreateHandCommand : ICommand
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
    public required List<ParticipantDto> Participants { get; init; }
}

public record struct CreateHandResponse : ICommandResponse
{
    public required Guid Uid { get; init; }
}

public class CreateHandHandler(
    IRepository repository,
    IEventDispatcher eventDispatcher,
    IRandomizer randomizer,
    IEvaluator evaluator
) : ICommandHandler<CreateHandCommand, CreateHandResponse>
{
    public async Task<CreateHandResponse> HandleAsync(CreateHandCommand command)
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

        return new CreateHandResponse
        {
            Uid = hand.Uid
        };
    }

    private Participant DeserializeParticipant(ParticipantDto dto)
    {
        return new Participant
        {
            Nickname = dto.Nickname,
            Seat = dto.Seat,
            Stack = dto.Stack
        };
    }
}
