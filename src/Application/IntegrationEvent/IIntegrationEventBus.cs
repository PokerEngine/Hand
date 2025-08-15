namespace Application.IntegrationEvent;

public interface IIntegrationEventBus
{
    Task Connect();
    Task Disconnect();
    Task Subscribe<T>(IIntegrationEventHandler<T> handler, IntegrationEventQueue queue) where T : IIntegrationEvent;
    Task Unsubscribe<T>(IIntegrationEventHandler<T> handler, IntegrationEventQueue queue) where T : IIntegrationEvent;
    Task Publish<T>(T integrationEvent, IntegrationEventQueue queue) where T : IIntegrationEvent;
}
