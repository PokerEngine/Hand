using Application.IntegrationEvent;
using System.Collections.Concurrent;

namespace Infrastructure.IntegrationEvent;

public class InMemoryIntegrationEventQueue : IIntegrationEventQueue
{
    private sealed class ChannelQueue
    {
        public readonly ConcurrentQueue<IIntegrationEvent> Queue = new();
        public readonly SemaphoreSlim Signal = new(0);
    }

    private readonly ConcurrentDictionary<IntegrationEventChannel, ChannelQueue> _channels = new();

    public Task EnqueueAsync(
        IIntegrationEvent integrationEvent,
        IntegrationEventChannel channel,
        CancellationToken cancellationToken = default
    )
    {
        if (integrationEvent is null)
            throw new ArgumentNullException(nameof(integrationEvent));

        var channelQueue = GetChannelQueue(channel);

        channelQueue.Queue.Enqueue(integrationEvent);
        channelQueue.Signal.Release();

        return Task.CompletedTask;
    }

    private ChannelQueue GetChannelQueue(IntegrationEventChannel channel)
    {
        return _channels.GetOrAdd(
            channel,
            _ => new ChannelQueue()
        );
    }
}
