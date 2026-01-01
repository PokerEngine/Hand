using Application.Event;
using Application.Repository;
using Domain.Entity;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Application.Command;
public record struct CreateHandCommand : ICommand
{
    public required string Type { get; init; }
    public required string Game { get; init; }
    public required int MaxSeat { get; init; }
    public required int SmallBlind { get; init; }
    public required int BigBlind { get; init; }
    public required int SmallBlindSeat { get; init; }
    public required int BigBlindSeat { get; init; }
    public required int ButtonSeat { get; init; }
    public required List<ParticipantDto> Participants { get; init; }
}

public record struct CreateHandResponse : ICommandResponse
{
    public required Guid HandUid { get; init; }
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
        var type = (HandType)Enum.Parse(typeof(HandType), command.Type);
        var game = (Game)Enum.Parse(typeof(Game), command.Game);

        var hand = Hand.FromScratch(
            uid: await repository.GetNextUidAsync(),
            type: type,
            game: game,
            maxSeat: command.MaxSeat,
            smallBlind: command.SmallBlind,
            bigBlind: command.BigBlind,
            smallBlindSeat: command.SmallBlindSeat,
            bigBlindSeat: command.BigBlindSeat,
            buttonSeat: command.ButtonSeat,
            participants: command.Participants.Select(DeserializeParticipant).ToList(),
            randomizer: randomizer,
            evaluator: evaluator
        );

        var events = hand.PullEvents();
        await repository.AddEventsAsync(hand.Uid, events);

        var context = new EventContext { HandUid = hand.Uid, HandType = hand.Type };
        foreach (var @event in events)
        {
            await eventDispatcher.DispatchAsync(@event, context);
        }

        return new CreateHandResponse
        {
            HandUid = hand.Uid
        };
    }

    private Participant DeserializeParticipant(ParticipantDto dto)
    {
        return new Participant(dto.Nickname, dto.Seat, dto.Stack);
    }
}
