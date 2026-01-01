using Application;
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
    [ProducesResponseType(typeof(CreateHandResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateHand([FromBody] CreateHandRequest request)
    {
        var command = new CreateHandCommand
        {
            Type = request.Type,
            Game = request.Game,
            MaxSeat = request.MaxSeat,
            SmallBlind = request.SmallBlind,
            BigBlind = request.BigBlind,
            SmallBlindSeat = request.SmallBlindSeat,
            BigBlindSeat = request.BigBlindSeat,
            ButtonSeat = request.ButtonSeat,
            Participants = request.Participants
        };
        var response = await commandDispatcher.DispatchAsync<CreateHandCommand, CreateHandResponse>(command);
        return CreatedAtAction(nameof(GetHandByUid), new { uid = response.HandUid }, response);
    }

    [HttpPost("{uid:guid}/start")]
    [ProducesResponseType(typeof(StartHandResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StartHand(Guid uid)
    {
        var command = new StartHandCommand
        {
            HandUid = uid
        };
        var response = await commandDispatcher.DispatchAsync<StartHandCommand, StartHandResponse>(command);
        return Ok(response);
    }

    [HttpPost("{uid:guid}/commit-decision/{nickname}")]
    [ProducesResponseType(typeof(CommitDecisionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CommitDecision(Guid uid, string nickname, [FromBody] CommitDecisionRequest request)
    {
        var command = new CommitDecisionCommand
        {
            HandUid = uid,
            Nickname = nickname,
            DecisionType = request.DecisionType,
            DecisionAmount = request.DecisionAmount
        };
        var response = await commandDispatcher.DispatchAsync<CommitDecisionCommand, CommitDecisionResponse>(command);
        return Ok(response);
    }

    [HttpGet("{uid:guid}")]
    [ProducesResponseType(typeof(GetHandByUidResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHandByUid(Guid uid)
    {
        var query = new GetHandByUidQuery { HandUid = uid };
        var response = await queryDispatcher.DispatchAsync<GetHandByUidQuery, GetHandByUidResponse>(query);
        return Ok(response);
    }
}

public record struct CreateHandRequest
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

public record struct CommitDecisionRequest
{
    public required string DecisionType { get; init; }
    public required int DecisionAmount { get; init; }
}
