namespace Application.IntegrationEvent;

public interface IIntegrationEventBus
{
    public void Connect();

    public void Disconnect();

    public void Subscribe<T>(IIntegrationEventHandler<T> handler, IntegrationEventQueue queue) where T : IIntegrationEvent;

    public void Unsubscribe<T>(IIntegrationEventHandler<T> handler, IntegrationEventQueue queue) where T : IIntegrationEvent;

    public void Publish<T>(T integrationEvent, IntegrationEventQueue queue) where T : IIntegrationEvent;
}
