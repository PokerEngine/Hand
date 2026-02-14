using Application.Repository;
using Application.UnitOfWork;
using Domain.Entity;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Application.Command;

public record SubmitPlayerActionCommand : ICommand
{
    public required Guid Uid { get; init; }
    public required string Nickname { get; init; }
    public required string Type { get; init; }
    public required int Amount { get; init; }
}

public record SubmitPlayerActionResponse : ICommandResponse
{
    public required Guid Uid { get; init; }
    public required string Nickname { get; init; }
}

public class SubmitPlayerActionHandler(
    IRepository repository,
    IUnitOfWork unitOfWork,
    IRandomizer randomizer,
    IEvaluator evaluator
) : ICommandHandler<SubmitPlayerActionCommand, SubmitPlayerActionResponse>
{
    public async Task<SubmitPlayerActionResponse> HandleAsync(SubmitPlayerActionCommand command)
    {
        var action = new PlayerAction(
            type: (PlayerActionType)Enum.Parse(typeof(PlayerActionType), command.Type),
            amount: command.Amount
        );

        var hand = Hand.FromEvents(
            uid: command.Uid,
            randomizer: randomizer,
            evaluator: evaluator,
            events: await repository.GetEventsAsync(command.Uid)
        );

        hand.SubmitPlayerAction(command.Nickname, action);

        unitOfWork.RegisterHand(hand);
        await unitOfWork.CommitAsync();

        return new SubmitPlayerActionResponse
        {
            Uid = hand.Uid,
            Nickname = command.Nickname,
        };
    }
}
