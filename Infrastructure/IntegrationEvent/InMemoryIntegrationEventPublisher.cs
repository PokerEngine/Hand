using Application.IntegrationEvent;

namespace Infrastructure.IntegrationEvent;

public class InMemoryIntegrationEventPublisher(ILogger<InMemoryIntegrationEventPublisher> logger) : IIntegrationEventPublisher
{
    public Task PublishAsync(
        IIntegrationEvent integrationEvent,
        IntegrationEventRoutingKey routingKey,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "In-memory publishing {IntegrationEvent} to {RoutingKey}",
            integrationEvent,
            routingKey
        );
        return Task.CompletedTask;
    }
}
