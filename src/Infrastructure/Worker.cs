using Application.IntegrationEvent;
using Application.Repository;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;

namespace Infrastructure;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IRepository _repository;
    private readonly IIntegrationEventBus _integrationEventBus;

    private readonly HandCreateIntegrationEventHandler _handCreateHandler;
    private readonly HandStartIntegrationEventHandler _handStartHandler;
    private readonly PlayerConnectIntegrationEventHandler _playerConnectHandler;
    private readonly PlayerDisconnectIntegrationEventHandler _playerDisconnectHandler;
    private readonly DecisionCommitIntegrationEventHandler _decisionCommitHandler;

    public Worker(
        ILogger<Worker> logger,
        IRepository repository,
        IIntegrationEventBus integrationEventBus,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        _logger = logger;
        _repository = repository;
        _integrationEventBus = integrationEventBus;

        _handCreateHandler = new HandCreateIntegrationEventHandler(
            integrationEventBus: _integrationEventBus,
            repository: _repository,
            randomizer: randomizer,
            evaluator: evaluator
        );
        _handStartHandler = new HandStartIntegrationEventHandler(
            integrationEventBus: _integrationEventBus,
            repository: _repository,
            randomizer: randomizer,
            evaluator: evaluator
        );
        _playerConnectHandler = new PlayerConnectIntegrationEventHandler(
            integrationEventBus: _integrationEventBus,
            repository: _repository,
            randomizer: randomizer,
            evaluator: evaluator
        );
        _playerDisconnectHandler = new PlayerDisconnectIntegrationEventHandler(
            integrationEventBus: _integrationEventBus,
            repository: _repository,
            randomizer: randomizer,
            evaluator: evaluator
        );
        _decisionCommitHandler = new DecisionCommitIntegrationEventHandler(
            integrationEventBus: _integrationEventBus,
            repository: _repository,
            randomizer: randomizer,
            evaluator: evaluator
        );
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);

        _integrationEventBus.Subscribe(_handCreateHandler, new IntegrationEventQueue("hand.hand-create"));
        _integrationEventBus.Subscribe(_handStartHandler, new IntegrationEventQueue("hand.hand-start"));
        _integrationEventBus.Subscribe(_playerConnectHandler, new IntegrationEventQueue("hand.player-connect"));
        _integrationEventBus.Subscribe(_playerDisconnectHandler, new IntegrationEventQueue("hand.player-disconnect"));
        _integrationEventBus.Subscribe(_decisionCommitHandler, new IntegrationEventQueue("hand.decision-commit"));

        _repository.Connect();
        _integrationEventBus.Connect();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _integrationEventBus.Disconnect();
        _repository.Disconnect();

        _integrationEventBus.Unsubscribe(_handCreateHandler, new IntegrationEventQueue("hand.hand-create"));
        _integrationEventBus.Unsubscribe(_handStartHandler, new IntegrationEventQueue("hand.hand-start"));
        _integrationEventBus.Unsubscribe(_playerConnectHandler, new IntegrationEventQueue("hand.player-connect"));
        _integrationEventBus.Unsubscribe(_playerDisconnectHandler, new IntegrationEventQueue("hand.player-disconnect"));
        _integrationEventBus.Unsubscribe(_decisionCommitHandler, new IntegrationEventQueue("hand.decision-commit"));

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
