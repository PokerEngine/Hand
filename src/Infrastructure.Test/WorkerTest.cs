using Application.IntegrationEvent;
using Application.Repository;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Infrastructure.IntegrationEvent;
using Infrastructure.Repository;
using Infrastructure.Service.Evaluator;
using Infrastructure.Service.Randomizer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Test;

public class TestHandIsCreatedIntegrationEventHandler : IIntegrationEventHandler<HandIsCreatedIntegrationEvent>
{
    public readonly List<HandIsCreatedIntegrationEvent> IntegrationEvents = [];

    public async Task Handle(HandIsCreatedIntegrationEvent integrationEvent)
    {
        IntegrationEvents.Add(integrationEvent);
        await Task.CompletedTask;
    }
}

public class WorkerTest
{
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

        await integrationEventBus.Unsubscribe(testHandIsCreatedHandler, new IntegrationEventQueue("hand.hand-created"));

        Assert.Single(testHandIsCreatedHandler.IntegrationEvents);
        Assert.Equal(handCreateEvent.Game, testHandIsCreatedHandler.IntegrationEvents[0].Game);
        Assert.Equal(handCreateEvent.SmallBlind, testHandIsCreatedHandler.IntegrationEvents[0].SmallBlind);
        Assert.Equal(handCreateEvent.BigBlind, testHandIsCreatedHandler.IntegrationEvents[0].BigBlind);
        Assert.Equal(handCreateEvent.Participants, testHandIsCreatedHandler.IntegrationEvents[0].Participants);
        Assert.Equal(handCreateEvent.TableUid, testHandIsCreatedHandler.IntegrationEvents[0].TableUid);
        Assert.Equal(handCreateEvent.HandUid, testHandIsCreatedHandler.IntegrationEvents[0].HandUid);

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

                services.AddSingleton<IIntegrationEventBus, InMemoryIntegrationEventBus>();

                services.Configure<MongoRepositoryOptions>(configuration.GetSection("Mongo"));
                services.AddSingleton<IRepository, MongoRepository>();

                services.AddSingleton<IRandomizer, BuiltInRandomizer>();

                services.Configure<PokerStoveEvaluatorOptions>(configuration.GetSection("PokerStove"));
                services.AddSingleton<IEvaluator, PokerStoveEvaluator>();
            }).Build();
    }
}
