namespace Application.IntegrationEvent;

public interface IIntegrationEventHandler<in T> where T : IIntegrationEvent
{
    public void Handle(T integrationEvent);
}