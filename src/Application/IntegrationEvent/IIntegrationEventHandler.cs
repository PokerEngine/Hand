namespace Application.IntegrationEvent;

public interface IIntegrationEventHandler<in T> where T : IIntegrationEvent
{
    public Task Handle(T integrationEvent);
}
