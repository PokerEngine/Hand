using Application.Repository;
using Domain.Entity;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Application.Query;

public record struct GetHandByUidQuery : IQuery
{
    public required Guid Uid { get; init; }
}

public record struct GetHandByUidResponse : IQueryResponse
{
    public required Guid Uid { get; init; }
    public required Guid TableUid { get; init; }
    public required string TableType { get; init; }
    public required string Game { get; init; }
    public required int SmallBlind { get; init; }
    public required int BigBlind { get; init; }
    public required int MaxSeat { get; init; }
    public required int SmallBlindSeat { get; init; }
    public required int BigBlindSeat { get; init; }
    public required int ButtonSeat { get; init; }
    public required GetHandByUidStateResponse State { get; init; }
}

public record struct GetHandByUidStateResponse
{
    public required List<GetHandByUidStatePlayerResponse> Players { get; init; }
    public required string BoardCards { get; init; }
    public required Chips CommittedPotAmount { get; init; }
}

public record struct GetHandByUidStatePlayerResponse
{
    public required string Nickname { get; init; }
    public required int Seat { get; init; }
    public required int Stack { get; init; }
    public required string HoleCards { get; init; }
    public required int UncommittedPotAmount { get; init; }
    public required bool IsFolded { get; init; }
}

public class GetHandByUidHandler(
    IRepository repository,
    IRandomizer randomizer,
    IEvaluator evaluator
) : IQueryHandler<GetHandByUidQuery, GetHandByUidResponse>
{
    public async Task<GetHandByUidResponse> HandleAsync(GetHandByUidQuery query)
    {
        var hand = Hand.FromEvents(
            uid: query.Uid,
            randomizer: randomizer,
            evaluator: evaluator,
            events: await repository.GetEventsAsync(query.Uid)
        );
        var state = hand.GetState();

        return new GetHandByUidResponse
        {
            Uid = hand.Uid,
            TableUid = hand.TableUid,
            TableType = hand.TableType.ToString(),
            Game = hand.Rules.Game.ToString(),
            SmallBlind = hand.Rules.SmallBlind,
            BigBlind = hand.Rules.BigBlind,
            MaxSeat = hand.Table.Positions.Max,
            SmallBlindSeat = hand.Table.Positions.SmallBlind,
            BigBlindSeat = hand.Table.Positions.BigBlind,
            ButtonSeat = hand.Table.Positions.Button,
            State = new GetHandByUidStateResponse
            {
                Players = state.Players.Select(SerializePlayerState).ToList(),
                BoardCards = state.BoardCards.ToString(),
                CommittedPotAmount = state.CommittedPotAmount
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
            UncommittedPotAmount = player.UncommittedPotAmount,
            IsFolded = player.IsFolded
        };
    }
}
