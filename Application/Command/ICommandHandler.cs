namespace Application.Command;

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand
    where TResponse : ICommandResponse
{
    Task<TResponse> HandleAsync(TCommand command);
}
