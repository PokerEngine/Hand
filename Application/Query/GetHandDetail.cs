using Application.Storage;

namespace Application.Query;

public record GetHandDetailQuery : IQuery
{
    public required Guid Uid { get; init; }
}

public record GetHandDetailResponse : IQueryResponse
{
    public required Guid Uid { get; init; }
    public required GetHandDetailResponseRules Rules { get; init; }
    public required GetHandDetailResponseTable Table { get; init; }
    public required GetHandDetailResponsePot Pot { get; init; }
}

public record GetHandDetailResponseRules
{
    public required string Game { get; init; }
    public required int SmallBlind { get; init; }
    public required int BigBlind { get; init; }
    public required int MaxSeat { get; init; }
}

public record GetHandDetailResponseTable
{
    public required GetHandDetailResponsePositions Positions { get; init; }
    public required List<GetHandDetailResponsePlayer> Players { get; init; }
    public required string BoardCards { get; init; }
}

public record GetHandDetailResponsePositions
{
    public required int SmallBlindSeat { get; init; }
    public required int BigBlindSeat { get; init; }
    public required int ButtonSeat { get; init; }
}

public record GetHandDetailResponsePlayer
{
    public required int Seat { get; init; }
    public required string Nickname { get; init; }
    public required int Stack { get; init; }
    public required string HoleCards { get; init; }
    public required bool IsFolded { get; init; }
}

public record GetHandDetailResponsePot
{
    public required int Ante { get; init; }
    public required List<GetHandDetailResponseBet> CollectedBets { get; init; }
    public required List<GetHandDetailResponseBet> CurrentBets { get; init; }
    public required List<GetHandDetailResponseAward> Awards { get; init; }
}

public record GetHandDetailResponseBet
{
    public required string Nickname { get; init; }
    public required int Amount { get; init; }
}

public record GetHandDetailResponseAward
{
    public required List<string> Winners { get; init; }
    public required int Amount { get; init; }
}

public class GetHandDetailHandler(
    IStorage storage
) : IQueryHandler<GetHandDetailQuery, GetHandDetailResponse>
{
    public async Task<GetHandDetailResponse> HandleAsync(GetHandDetailQuery query)
    {
        var view = await storage.GetDetailViewAsync(query.Uid);

        return new GetHandDetailResponse
        {
            Uid = view.Uid,
            Rules = new GetHandDetailResponseRules
            {
                Game = view.Rules.Game,
                SmallBlind = view.Rules.SmallBlind,
                BigBlind = view.Rules.BigBlind,
                MaxSeat = view.Rules.MaxSeat
            },
            Table = SerializeTable(view.Table),
            Pot = SerializePot(view.Pot)
        };
    }

    private GetHandDetailResponseTable SerializeTable(DetailViewTable table)
    {
        return new GetHandDetailResponseTable
        {
            Positions = new GetHandDetailResponsePositions
            {
                SmallBlindSeat = table.Positions.SmallBlindSeat,
                BigBlindSeat = table.Positions.BigBlindSeat,
                ButtonSeat = table.Positions.ButtonSeat,
            },
            Players = table.Players.Select(SerializePlayer).ToList(),
            BoardCards = table.BoardCards
        };
    }

    private GetHandDetailResponsePlayer SerializePlayer(DetailViewPlayer player)
    {
        return new GetHandDetailResponsePlayer
        {
            Nickname = player.Nickname,
            Seat = player.Seat,
            Stack = player.Stack,
            HoleCards = player.HoleCards,
            IsFolded = player.IsFolded
        };
    }

    private GetHandDetailResponsePot SerializePot(DetailViewPot pot)
    {
        return new GetHandDetailResponsePot
        {
            Ante = pot.Ante,
            CollectedBets = pot.CollectedBets.Select(SerializeBet).ToList(),
            CurrentBets = pot.CurrentBets.Select(SerializeBet).ToList(),
            Awards = pot.Awards.Select(SerializeAward).ToList()
        };
    }

    private GetHandDetailResponseBet SerializeBet(DetailViewBet bet)
    {
        return new GetHandDetailResponseBet
        {
            Nickname = bet.Nickname,
            Amount = bet.Amount
        };
    }

    private GetHandDetailResponseAward SerializeAward(DetailViewAward award)
    {
        return new GetHandDetailResponseAward
        {
            Winners = award.Winners,
            Amount = award.Amount
        };
    }
}
