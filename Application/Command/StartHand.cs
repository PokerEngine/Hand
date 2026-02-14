using Application.Repository;
using Application.UnitOfWork;
using Domain.Entity;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Application.Command;

public record StartHandCommand : ICommand
{
    public required Guid TableUid { get; init; }
    public required string TableType { get; init; }
    public required StartHandCommandRules Rules { get; init; }
    public required StartHandCommandTable Table { get; init; }
}

public record StartHandCommandRules
{
    public required string Game { get; init; }
    public required int MaxSeat { get; init; }
    public required int SmallBlind { get; init; }
    public required int BigBlind { get; init; }
}

public record StartHandCommandTable
{
    public required StartHandCommandPositions Positions { get; init; }
    public required List<StartHandCommandPlayer> Players { get; init; }
}

public record StartHandCommandPositions
{
    public required int SmallBlindSeat { get; init; }
    public required int BigBlindSeat { get; init; }
    public required int ButtonSeat { get; init; }
}

public record StartHandCommandPlayer
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
    IUnitOfWork unitOfWork,
    IRandomizer randomizer,
    IEvaluator evaluator
) : ICommandHandler<StartHandCommand, StartHandResponse>
{
    public async Task<StartHandResponse> HandleAsync(StartHandCommand command)
    {
        var tableType = (TableType)Enum.Parse(typeof(TableType), command.TableType);
        var game = (Game)Enum.Parse(typeof(Game), command.Rules.Game);

        var rules = new Rules
        {
            Game = game,
            MaxSeat = command.Rules.MaxSeat,
            SmallBlind = command.Rules.SmallBlind,
            BigBlind = command.Rules.BigBlind
        };
        var positions = new Positions
        {
            SmallBlindSeat = command.Table.Positions.SmallBlindSeat,
            BigBlindSeat = command.Table.Positions.BigBlindSeat,
            ButtonSeat = command.Table.Positions.ButtonSeat
        };
        var players = command.Table.Players.Select(DeserializePlayer).ToList();

        var hand = Hand.FromScratch(
            uid: await repository.GetNextUidAsync(),
            tableUid: command.TableUid,
            tableType: tableType,
            rules: rules,
            positions: positions,
            players: players,
            randomizer: randomizer,
            evaluator: evaluator
        );
        hand.Start();

        unitOfWork.RegisterHand(hand);
        await unitOfWork.CommitAsync();

        return new StartHandResponse
        {
            Uid = hand.Uid
        };
    }

    private Participant DeserializePlayer(StartHandCommandPlayer player)
    {
        return new Participant
        {
            Nickname = player.Nickname,
            Seat = player.Seat,
            Stack = player.Stack
        };
    }
}
