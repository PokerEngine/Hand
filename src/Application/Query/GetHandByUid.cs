using Application.Repository;
using Domain.Entity;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;

namespace Application.Query;

public record struct GetHandByUidQuery : IQuery
{
    public required Guid HandUid { get; init; }
}

public record struct GetHandByUidResponse : IQueryResponse
{
    public required Guid HandUid { get; init; }
    public required List<ParticipantDto> Participants { get; init; }
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

        return new GetHandByUidResponse
        {
            HandUid = hand.Uid,
            Participants = hand.Table.Players.Select(SerializeParticipant).ToList()
        };
    }

    private ParticipantDto SerializeParticipant(Player player)
    {
        return new ParticipantDto
        {
            Nickname = player.Nickname,
            Seat = player.Seat,
            Stack = player.Stack
        };
    }
}
