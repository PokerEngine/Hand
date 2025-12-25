namespace Application.IntegrationEvent;

public interface IIntegrationEventQueue
{
    Task EnqueueAsync(
        IIntegrationEvent integrationEvent,
        IntegrationEventChannel channel,
        CancellationToken cancellationToken = default
    );
}
