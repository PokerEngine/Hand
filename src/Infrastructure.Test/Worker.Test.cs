using Application.IntegrationEvent;
using Application.Repository;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Infrastructure.IntegrationEvent;
using Infrastructure.Repository;
using Infrastructure.Service.Evaluator;
using Infrastructure.Service.Randomizer;
using Infrastructure.Test.Fixture;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Test;

public class TestHandIsCreatedIntegrationEventHandler : IIntegrationEventHandler<HandIsCreatedIntegrationEvent>
{
    public readonly List<HandIsCreatedIntegrationEvent> Events = new();
    public readonly TaskCompletionSource<bool> EventHandledTcs = new();

    public async Task Handle(HandIsCreatedIntegrationEvent integrationEvent)
    {
        Events.Add(integrationEvent);
        EventHandledTcs.TrySetResult(true);
        await Task.CompletedTask;
    }
}

public class WorkerTest : IClassFixture<MongoDbFixture>, IClassFixture<RabbitMqFixture>, IDisposable
{
    private readonly MongoDbFixture _dbFixture;

    public WorkerTest(MongoDbFixture dbFixture)
    {
        _dbFixture = dbFixture;
        _dbFixture.Database.CreateCollection("events");
    }

    public void Dispose()
    {
        _dbFixture.Database.DropCollection("events");
    }

    [Fact]
    public async Task TestHandCreate()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        var host = GetHost();

        await host.StartAsync(cts.Token);

        var integrationEventBus = host.Services.GetRequiredService<IIntegrationEventBus>();

        var testHandIsCreatedHandler = new TestHandIsCreatedIntegrationEventHandler();
        await integrationEventBus.Subscribe(testHandIsCreatedHandler, new IntegrationEventQueue("hand.hand-created"));

        var handCreateEvent = new HandCreateIntegrationEvent(
            Game: "HoldemNoLimit6Max",
            SmallBlind: 5,
            BigBlind: 10,
            Participants: [
                new IntegrationEventParticipant("SmallBlind", "SmallBlind", 1000),
                new IntegrationEventParticipant("BigBlind", "BigBlind", 1000),
                new IntegrationEventParticipant("Button", "Button", 1000)
            ],
            TableUid: Guid.NewGuid(),
            HandUid: Guid.NewGuid(),
            OccuredAt: DateTime.Now
        );
        await integrationEventBus.Publish(handCreateEvent, new IntegrationEventQueue("hand.hand-create"));
        await Task.WhenAny(testHandIsCreatedHandler.EventHandledTcs.Task, Task.Delay(500, cts.Token));

        await integrationEventBus.Unsubscribe(testHandIsCreatedHandler, new IntegrationEventQueue("hand.hand-created"));

        Assert.Single(testHandIsCreatedHandler.Events);
        Assert.Equal(handCreateEvent.Game, testHandIsCreatedHandler.Events[0].Game);
        Assert.Equal(handCreateEvent.SmallBlind, testHandIsCreatedHandler.Events[0].SmallBlind);
        Assert.Equal(handCreateEvent.BigBlind, testHandIsCreatedHandler.Events[0].BigBlind);
        Assert.Equal(handCreateEvent.Participants, testHandIsCreatedHandler.Events[0].Participants);
        Assert.Equal(handCreateEvent.TableUid, testHandIsCreatedHandler.Events[0].TableUid);
        Assert.Equal(handCreateEvent.HandUid, testHandIsCreatedHandler.Events[0].HandUid);

        await host.StopAsync(cts.Token);
    }

    private IHost GetHost()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                configuration = config.Build();
            })
            .ConfigureServices(services =>
            {
                services.AddHostedService<Worker>();

                services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMQ"));
                services.AddSingleton<IIntegrationEventBus, RabbitMqIntegrationEventBus>();

                services.Configure<MongoDbOptions>(configuration.GetSection("MongoDB"));
                services.AddSingleton<IRepository, MongoDbRepository>();

                services.AddSingleton<IRandomizer, BuiltInRandomizer>();

                services.Configure<PokerStoveOptions>(configuration.GetSection("PokerStove"));
                services.AddSingleton<IEvaluator, PokerStoveEvaluator>();
            }).Build();
    }
}
