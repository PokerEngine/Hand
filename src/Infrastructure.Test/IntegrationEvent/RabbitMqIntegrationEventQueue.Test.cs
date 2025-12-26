using Application.IntegrationEvent;
using Infrastructure.IntegrationEvent;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Test.IntegrationEvent;

[Trait("Category", "Integration")]
public class RabbitMqIntegrationEventQueueTest(RabbitMqFixture fixture) : IClassFixture<RabbitMqFixture>
{
    [Fact]
    public async Task EnqueueAsync_WhenConnected_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var integrationEventQueue = CreateIntegrationEventQueue();

        var integrationEvent = new TestIntegrationEvent
        {
            HandUid = Guid.NewGuid(),
            Name = "Test Event",
            Number = 100500,
            OccuredAt = DateTime.UtcNow
        };

        // Act
        await integrationEventQueue.EnqueueAsync(integrationEvent, IntegrationEventChannel.Hand);

        // Assert
        await using var connection = await fixture.CreateFactory().CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        var result = await channel.BasicGetAsync(
            queue: "Hand",
            autoAck: true
        );

        Assert.NotNull(result);
        var json = Encoding.UTF8.GetString(result.Body.ToArray());
        var envelope = JsonSerializer.Deserialize<IntegrationEventEnvelope>(json);

        Assert.NotNull(envelope);
        Assert.Equal(typeof(TestIntegrationEvent).AssemblyQualifiedName, envelope.Type);
        Assert.Equal(integrationEvent.OccuredAt, envelope.OccurredAt);
        Assert.True(envelope.Data.ValueKind == JsonValueKind.Object);
    }

    private IIntegrationEventQueue CreateIntegrationEventQueue()
    {
        var options = Options.Create(fixture.CreateOptions());
        return new RabbitMqIntegrationEventQueue(
            options,
            NullLogger<RabbitMqIntegrationEventQueue>.Instance
        );
    }
}

internal record TestIntegrationEvent : IIntegrationEvent
{
    public required Guid HandUid { get; init; }
    public required string Name { get; init; }
    public required int Number { get; init; }
    public required DateTime OccuredAt { get; init; }
}
