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
builder.Services.AddSingleton<IRepository, InMemoryRepository>();
builder.Services.AddSingleton<IIntegrationEventBus, InMemoryIntegrationEventBus>();
builder.Services.AddSingleton<IRandomizer, BuiltInRandomizer>();
builder.Services.AddSingleton<IEvaluator, PokerStoveEvaluator>();

var host = builder.Build();
host.Run();
