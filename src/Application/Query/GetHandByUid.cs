using Application.Repository;
using Domain.Entity;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Application.Query;

public record GetHandByUidQuery : IQuery
{
    public required Guid Uid { get; init; }
}

public record GetHandByUidResponse : IQueryResponse
{
    public required Guid Uid { get; init; }
    public required Guid TableUid { get; init; }
    public required string TableType { get; init; }
    public required string Game { get; init; }
    public required int SmallBlind { get; init; }
    public required int BigBlind { get; init; }
    public required int SmallBlindSeat { get; init; }
    public required int BigBlindSeat { get; init; }
    public required int ButtonSeat { get; init; }
    public required int MaxSeat { get; init; }
    public required GetHandByUidStateResponse State { get; init; }
}

public record GetHandByUidStateResponse
{
    public required GetHandByUidTableStateResponse Table { get; init; }
    public required GetHandByUidPotStateResponse Pot { get; init; }
}

public record GetHandByUidTableStateResponse
{
    public required List<GetHandByUidPlayerStateResponse> Players { get; init; }
    public required string BoardCards { get; init; }
}

public record GetHandByUidPlayerStateResponse
{
    public required int Seat { get; init; }
    public required string Nickname { get; init; }
    public required int Stack { get; init; }
    public required string HoleCards { get; init; }
    public required bool IsFolded { get; init; }
}

public record GetHandByUidPotStateResponse
{
    public required int Ante { get; init; }
    public required List<GetHandByUidBetStateResponse> CollectedBets { get; init; }
    public required List<GetHandByUidBetStateResponse> CurrentBets { get; init; }
    public required List<GetHandByUidAwardStateResponse> Awards { get; init; }
}

public record GetHandByUidBetStateResponse
{
    public required string Nickname { get; init; }
    public required int Amount { get; init; }
}

public record GetHandByUidAwardStateResponse
{
    public required List<string> Winners { get; init; }
    public required int Amount { get; init; }
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
                Table = SerializeTableState(state.Table),
                Pot = SerializePotState(state.Pot)
            }
        };
    }

    private GetHandByUidTableStateResponse SerializeTableState(TableState state)
    {
        return new GetHandByUidTableStateResponse
        {
            Players = state.Players.Select(SerializePlayerState).ToList(),
            BoardCards = state.BoardCards
        };
    }

    private GetHandByUidPlayerStateResponse SerializePlayerState(PlayerState state)
    {
        return new GetHandByUidPlayerStateResponse
        {
            Nickname = state.Nickname,
            Seat = state.Seat,
            Stack = state.Stack,
            HoleCards = state.HoleCards,
            IsFolded = state.IsFolded
        };
    }

    private GetHandByUidPotStateResponse SerializePotState(PotState state)
    {
        return new GetHandByUidPotStateResponse
        {
            Ante = state.Ante,
            CollectedBets = state.CollectedBets.Select(SerializeBetState).ToList(),
            CurrentBets = state.CurrentBets.Select(SerializeBetState).ToList(),
            Awards = state.Awards.Select(SerializeAwardState).ToList()
        };
    }

    private GetHandByUidBetStateResponse SerializeBetState(BetState state)
    {
        return new GetHandByUidBetStateResponse
        {
            Nickname = state.Nickname,
            Amount = state.Amount
        };
    }

    private GetHandByUidAwardStateResponse SerializeAwardState(AwardState state)
    {
        return new GetHandByUidAwardStateResponse
        {
            Winners = state.Winners.Select(x => x.ToString()).ToList(),
            Amount = state.Amount
        };
    }
}
