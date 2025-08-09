namespace Application.IntegrationEvent;

public interface IIntegrationEventBus
{
    public Task Connect();
    public Task Disconnect();
    public Task Subscribe<T>(IIntegrationEventHandler<T> handler, IntegrationEventQueue queue) where T : IIntegrationEvent;
    public Task Unsubscribe<T>(IIntegrationEventHandler<T> handler, IntegrationEventQueue queue) where T : IIntegrationEvent;
    public Task Publish<T>(T integrationEvent, IntegrationEventQueue queue) where T : IIntegrationEvent;
}
