using Application.Repository;
using Domain.Entity;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Application.Query;

public record struct GetHandByUidQuery : IQuery
{
    public required Guid HandUid { get; init; }
}

public record struct GetHandByUidResponse : IQueryResponse
{
    public required Guid HandUid { get; init; }
    public required string Type { get; init; }
    public required string Game { get; init; }
    public required int MaxSeat { get; init; }
    public required int SmallBlind { get; init; }
    public required int BigBlind { get; init; }
    public required int SmallBlindSeat { get; init; }
    public required int BigBlindSeat { get; init; }
    public required int ButtonSeat { get; init; }
    public required GetHandByUidStateResponse State { get; init; }
}

public record struct GetHandByUidStateResponse
{
    public required List<GetHandByUidStatePlayerResponse> Players { get; init; }
    public required string BoardCards { get; init; }
    public required Dictionary<string, int> CurrentSidePot { get; init; }
    public required Dictionary<string, int> PreviousSidePot { get; init; }
}

public record struct GetHandByUidStatePlayerResponse
{
    public required string Nickname { get; init; }
    public required int Seat { get; init; }
    public required int Stack { get; init; }
    public required string HoleCards { get; init; }
    public required bool IsFolded { get; init; }
}

public class GetHandByUidHandler(
    IRepository repository,
    IRandomizer randomizer,
    IEvaluator evaluator
) : IQueryHandler<GetHandByUidQuery, GetHandByUidResponse>
{
    public async Task<GetHandByUidResponse> HandleAsync(GetHandByUidQuery command)
    {
        var hand = Hand.FromEvents(
            uid: command.HandUid,
            randomizer: randomizer,
            evaluator: evaluator,
            events: await repository.GetEventsAsync(command.HandUid)
        );
        var state = hand.GetState();

        return new GetHandByUidResponse
        {
            HandUid = hand.Uid,
            Type = hand.Type.ToString(),
            Game = hand.Game.ToString(),
            MaxSeat = hand.Table.MaxSeat,
            SmallBlind = hand.Pot.SmallBlind,
            BigBlind = hand.Pot.BigBlind,
            SmallBlindSeat = hand.Table.SmallBlindSeat,
            BigBlindSeat = hand.Table.BigBlindSeat,
            ButtonSeat = hand.Table.ButtonSeat,
            State = new GetHandByUidStateResponse
            {
                Players = state.Players.Select(SerializePlayerState).ToList(),
                BoardCards = state.BoardCards.ToString(),
                CurrentSidePot = state.CurrentSidePot.ToDictionary(p => (string)p.Key, p => (int)p.Value),
                PreviousSidePot = state.PreviousSidePot.ToDictionary(p => (string)p.Key, p => (int)p.Value)
            }
        };
    }

    private GetHandByUidStatePlayerResponse SerializePlayerState(StatePlayer player)
    {
        return new GetHandByUidStatePlayerResponse
        {
            Nickname = player.Nickname,
            Seat = player.Seat,
            Stack = player.Stack,
            HoleCards = player.HoleCards.ToString(),
            IsFolded = player.IsFolded
        };
    }
}
