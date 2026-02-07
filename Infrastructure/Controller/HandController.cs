using Application.Command;
using Application.Query;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Controller;

[ApiController]
[Route("api/hand")]
[Produces("application/json")]
public class HandController(
    ICommandDispatcher commandDispatcher,
    IQueryDispatcher queryDispatcher
) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(StartHandResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StartHand([FromBody] StartHandRequest request)
    {
        var command = new StartHandCommand
        {
            TableUid = request.TableUid,
            TableType = request.TableType,
            Rules = new StartHandCommandRules
            {
                Game = request.Rules.Game,
                MaxSeat = request.Rules.MaxSeat,
                SmallBlind = request.Rules.SmallBlind,
                BigBlind = request.Rules.BigBlind,
            },
            Table = new StartHandCommandTable
            {
                Positions = new StartHandCommandPositions
                {
                    SmallBlindSeat = request.Table.Positions.SmallBlindSeat,
                    BigBlindSeat = request.Table.Positions.BigBlindSeat,
                    ButtonSeat = request.Table.Positions.ButtonSeat,
                },
                Participants = request.Table.Participants.Select(DeserializeParticipant).ToList()
            }
        };
        var response = await commandDispatcher.DispatchAsync<StartHandCommand, StartHandResponse>(command);
        return CreatedAtAction(nameof(GetHandDetail), new { uid = response.Uid }, response);
    }

    [HttpPost("{uid:guid}/submit-action/{nickname}")]
    [ProducesResponseType(typeof(SubmitPlayerActionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SubmitPlayerAction(Guid uid, string nickname, [FromBody] SubmitPlayerActionRequest request)
    {
        var command = new SubmitPlayerActionCommand
        {
            Uid = uid,
            Nickname = nickname,
            Type = request.Type,
            Amount = request.Amount
        };
        var response = await commandDispatcher.DispatchAsync<SubmitPlayerActionCommand, SubmitPlayerActionResponse>(command);
        return Ok(response);
    }

    [HttpGet("{uid:guid}")]
    [ProducesResponseType(typeof(GetHandDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHandDetail(Guid uid)
    {
        var query = new GetHandDetailQuery { Uid = uid };
        var response = await queryDispatcher.DispatchAsync<GetHandDetailQuery, GetHandDetailResponse>(query);
        return Ok(response);
    }

    private StartHandCommandParticipant DeserializeParticipant(StartHandRequestParticipant participant)
    {
        return new StartHandCommandParticipant
        {
            Nickname = participant.Nickname,
            Seat = participant.Seat,
            Stack = participant.Stack
        };
    }
}

public record StartHandRequest
{
    public required Guid TableUid { get; init; }
    public required string TableType { get; init; }
    public required StartHandRequestRules Rules { get; init; }
    public required StartHandRequestTable Table { get; init; }
}

public record StartHandRequestRules
{
    public required string Game { get; init; }
    public required int MaxSeat { get; init; }
    public required int SmallBlind { get; init; }
    public required int BigBlind { get; init; }
}

public record StartHandRequestTable
{
    public required StartHandRequestPositions Positions { get; init; }
    public required List<StartHandRequestParticipant> Participants { get; init; }
}

public record StartHandRequestPositions
{
    public required int SmallBlindSeat { get; init; }
    public required int BigBlindSeat { get; init; }
    public required int ButtonSeat { get; init; }
}

public record StartHandRequestParticipant
{
    public required string Nickname { get; init; }
    public required int Seat { get; init; }
    public required int Stack { get; init; }
}

public record SubmitPlayerActionRequest
{
    public required string Type { get; init; }
    public required int Amount { get; init; }
}
