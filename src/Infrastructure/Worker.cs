using Application.IntegrationEvent;
using Infrastructure.IntegrationEvent;
using Infrastructure.Repository;
using Infrastructure.Service.Evaluator;

namespace Infrastructure;

public class Worker : BackgroundService
{
    private static readonly InMemoryRepository Repository = new();
    private static readonly PokerStoveEvaluator Evaluator = new();
    private static readonly InMemoryIntegrationEventBus IntegrationEventBus = new();
    private static readonly IntegrationEventQueue IntegrationEventQueue = new("hand");
    private static readonly HandCreateIntegrationEventHandler HandCreateHandler = new(
        integrationEventBus: IntegrationEventBus,
        repository: Repository,
        evaluator: Evaluator
    );
    private static readonly HandStartIntegrationEventHandler HandStartHandler = new(
        integrationEventBus: IntegrationEventBus,
        repository: Repository,
        evaluator: Evaluator
    );
    private static readonly PlayerConnectIntegrationEventHandler PlayerConnectHandler = new(
        integrationEventBus: IntegrationEventBus,
        repository: Repository,
        evaluator: Evaluator
    );
    private static readonly PlayerDisconnectIntegrationEventHandler PlayerDisconnectHandler = new(
        integrationEventBus: IntegrationEventBus,
        repository: Repository,
        evaluator: Evaluator
    );
    private static readonly DecisionCommitIntegrationEventHandler DecisionCommitHandler = new(
        integrationEventBus: IntegrationEventBus,
        repository: Repository,
        evaluator: Evaluator
    );

    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);

        IntegrationEventBus.Subscribe(HandCreateHandler, IntegrationEventQueue);
        IntegrationEventBus.Subscribe(HandStartHandler, IntegrationEventQueue);
        IntegrationEventBus.Subscribe(PlayerConnectHandler, IntegrationEventQueue);
        IntegrationEventBus.Subscribe(PlayerDisconnectHandler, IntegrationEventQueue);
        IntegrationEventBus.Subscribe(DecisionCommitHandler, IntegrationEventQueue);
        Repository.Connect();
        IntegrationEventBus.Connect();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        IntegrationEventBus.Disconnect();
        Repository.Disconnect();
        IntegrationEventBus.Unsubscribe(HandCreateHandler, IntegrationEventQueue);
        IntegrationEventBus.Unsubscribe(HandStartHandler, IntegrationEventQueue);
        IntegrationEventBus.Unsubscribe(PlayerConnectHandler, IntegrationEventQueue);
        IntegrationEventBus.Unsubscribe(PlayerDisconnectHandler, IntegrationEventQueue);
        IntegrationEventBus.Unsubscribe(DecisionCommitHandler, IntegrationEventQueue);

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
