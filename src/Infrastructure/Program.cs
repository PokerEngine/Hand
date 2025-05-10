using Application.IntegrationEvent;
using Application.Repository;
using Domain.Service.Evaluator;
using Infrastructure;
using Infrastructure.IntegrationEvent;
using Infrastructure.Repository;
using Infrastructure.Service.Evaluator;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IRepository, InMemoryRepository>();
builder.Services.AddSingleton<IIntegrationEventBus, InMemoryIntegrationEventBus>();
builder.Services.AddSingleton<IEvaluator, PokerStoveEvaluator>();

var host = builder.Build();
host.Run();
