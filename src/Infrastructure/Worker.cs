using Application.IntegrationEvent;
using Application.Repository;
using Domain.Service.Evaluator;
using Infrastructure.IntegrationEvent;
using Infrastructure.Repository;
using Infrastructure.Service.Evaluator;

namespace Infrastructure;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IRepository _repository;
    private readonly IEvaluator _evaluator;
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly IntegrationEventQueue _integrationEventQueue;
    private readonly HandCreateIntegrationEventHandler _handCreateHandler;
    private readonly HandStartIntegrationEventHandler _handStartHandler;
    private readonly PlayerConnectIntegrationEventHandler _playerConnectHandler;
    private readonly PlayerDisconnectIntegrationEventHandler _playerDisconnectHandler;
    private readonly DecisionCommitIntegrationEventHandler _decisionCommitHandler;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;

        _repository = new InMemoryRepository();
        _evaluator = new PokerStoveEvaluator();
        _integrationEventBus = new InMemoryIntegrationEventBus();
        _integrationEventQueue = new IntegrationEventQueue("hand");
        _handCreateHandler = new HandCreateIntegrationEventHandler(
            integrationEventBus: _integrationEventBus,
            repository: _repository,
            evaluator: _evaluator
        );
        _handStartHandler = new HandStartIntegrationEventHandler(
            integrationEventBus: _integrationEventBus,
            repository: _repository,
            evaluator: _evaluator
        );
        _playerConnectHandler = new PlayerConnectIntegrationEventHandler(
            integrationEventBus: _integrationEventBus,
            repository: _repository,
            evaluator: _evaluator
        );
        _playerDisconnectHandler = new PlayerDisconnectIntegrationEventHandler(
            integrationEventBus: _integrationEventBus,
            repository: _repository,
            evaluator: _evaluator
        );
        _decisionCommitHandler = new DecisionCommitIntegrationEventHandler(
            integrationEventBus: _integrationEventBus,
            repository: _repository,
            evaluator: _evaluator
        );
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);

        _integrationEventBus.Subscribe(_handCreateHandler, _integrationEventQueue);
        _integrationEventBus.Subscribe(_handStartHandler, _integrationEventQueue);
        _integrationEventBus.Subscribe(_playerConnectHandler, _integrationEventQueue);
        _integrationEventBus.Subscribe(_playerDisconnectHandler, _integrationEventQueue);
        _integrationEventBus.Subscribe(_decisionCommitHandler, _integrationEventQueue);
        _repository.Connect();
        _integrationEventBus.Connect();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _integrationEventBus.Disconnect();
        _repository.Disconnect();
        _integrationEventBus.Unsubscribe(_handCreateHandler, _integrationEventQueue);
        _integrationEventBus.Unsubscribe(_handStartHandler, _integrationEventQueue);
        _integrationEventBus.Unsubscribe(_playerConnectHandler, _integrationEventQueue);
        _integrationEventBus.Unsubscribe(_playerDisconnectHandler, _integrationEventQueue);
        _integrationEventBus.Unsubscribe(_decisionCommitHandler, _integrationEventQueue);

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
