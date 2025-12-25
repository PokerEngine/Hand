using Infrastructure.IntegrationEvent;
using RabbitMQ.Client;
using Testcontainers.RabbitMq;

namespace Infrastructure.Test.IntegrationEvent;

public sealed class RabbitMqFixture : IAsyncLifetime
{
    private const string Username = "guest";
    private const string Password = "guest";
    private const int Port = 5672;

    private RabbitMqContainer Container { get; } =
        new RabbitMqBuilder()
            .WithImage("rabbitmq:4-management")
            .WithUsername(Username)
            .WithPassword(Password)
            .Build();

    public async Task InitializeAsync()
    {
        await Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }

    public ConnectionFactory CreateFactory()
    {
        return new ConnectionFactory
        {
            HostName = Container.Hostname,
            Port = Container.GetMappedPublicPort(Port),
            UserName = Username,
            Password = Password
        };
    }

    public RabbitMqIntegrationEventQueueOptions CreateOptions()
    {
        return new RabbitMqIntegrationEventQueueOptions
        {
            Host = Container.Hostname,
            Port = Container.GetMappedPublicPort(Port),
            Username = Username,
            Password = Password,
            VirtualHost = "/"
        };
    }
}
