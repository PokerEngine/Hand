using Application.IntegrationEvent;
using Application.Repository;
using Domain.Service.Evaluator;
using Infrastructure.IntegrationEvent;
using Infrastructure.Repository;
using Infrastructure.Service.Evaluator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Test;

public class WorkerTest
{
    [Fact]
    public async Task WorkerCreateHand()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddHostedService<Worker>();
                services.AddSingleton<IRepository, InMemoryRepository>();
                services.AddSingleton<IIntegrationEventBus, InMemoryIntegrationEventBus>();
                services.AddSingleton<IEvaluator, PokerStoveEvaluator>();
            })
            .Build();

        await host.StartAsync(cts.Token);

        var integrationEventBus = host.Services.GetRequiredService<IIntegrationEventBus>();

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

        await host.StopAsync();
    }
}
