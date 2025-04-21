using Application;
using Application.IntegrationEvent;
using Infrastructure.IntegrationEvent;
using Infrastructure.Repository;

namespace Infrastructure;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IRepository _repository;
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly IntegrationEventQueue _integrationEventQueue;
    private readonly HandCreationRequestedIntegrationEventHandler _handCreationRequestedHandler;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;

        _repository = new InMemoryRepository();
        _integrationEventBus = new InMemoryIntegrationEventBus();
        _integrationEventQueue = new IntegrationEventQueue("hand");
        _handCreationRequestedHandler = new HandCreationRequestedIntegrationEventHandler(
            integrationEventBus: _integrationEventBus,
            repository: _repository
        );
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);

        _integrationEventBus.Subscribe(_handCreationRequestedHandler, _integrationEventQueue);
        _repository.Connect();
        _integrationEventBus.Connect();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _integrationEventBus.Disconnect();
        _repository.Disconnect();
        _integrationEventBus.Unsubscribe(_handCreationRequestedHandler, _integrationEventQueue);

        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}