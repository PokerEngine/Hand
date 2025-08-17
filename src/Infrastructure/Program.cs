using Application.IntegrationEvent;
using Application.Repository;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Infrastructure;
using Infrastructure.IntegrationEvent;
using Infrastructure.Repository;
using Infrastructure.Service.Evaluator;
using Infrastructure.Service.Randomizer;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<IIntegrationEventBus, RabbitMqIntegrationEventBus>();

builder.Services.Configure<MongoDbOptions>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<IRepository, MongoDbRepository>();

builder.Services.AddSingleton<IRandomizer, BuiltInRandomizer>();

builder.Services.Configure<PokerStoveOptions>(builder.Configuration.GetSection("PokerStove"));
builder.Services.AddSingleton<IEvaluator, PokerStoveEvaluator>();

var host = builder.Build();
host.Run();

public class RabbitMqOptions
{
    public required string Host { get; set; }
    public required int Port { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string VirtualHost { get; set; }
}

public class MongoDbOptions
{
    public required string Host { get; set; }
    public required int Port { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Database { get; set; }
}

public class PokerStoveOptions
{
    public required string Path { get; set; }
}
