using Application.IntegrationEvent;
using Application.Repository;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Infrastructure.IntegrationEvent;
using Infrastructure.Repository;
using Infrastructure.Service.Evaluator;
using Infrastructure.Service.Randomizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Test;

public class TestHandIsCreatedIntegrationEventHandler : IIntegrationEventHandler<HandIsCreatedIntegrationEvent>
{
    public readonly List<HandIsCreatedIntegrationEvent> IntegrationEvents = [];

    public void Handle(HandIsCreatedIntegrationEvent integrationEvent)
    {
        IntegrationEvents.Add(integrationEvent);
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
        integrationEventBus.Subscribe(testHandIsCreatedHandler, new IntegrationEventQueue("hand.hand-created"));

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
        integrationEventBus.Publish(handCreateEvent, new IntegrationEventQueue("hand.hand-create"));

        integrationEventBus.Unsubscribe(testHandIsCreatedHandler, new IntegrationEventQueue("hand.hand-created"));

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
        return Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddHostedService<Worker>();
                services.AddSingleton<IRepository, InMemoryRepository>();
                services.AddSingleton<IIntegrationEventBus, InMemoryIntegrationEventBus>();
                services.AddSingleton<IRandomizer, BuiltInRandomizer>();
                services.AddSingleton<IEvaluator, PokerStoveEvaluator>();
            }).Build();
    }
}
