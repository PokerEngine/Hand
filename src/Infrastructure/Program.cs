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

builder.Services.AddSingleton<IIntegrationEventBus, InMemoryIntegrationEventBus>();

builder.Services.Configure<MongoRepositoryOptions>(builder.Configuration.GetSection("MongoRepository"));
builder.Services.AddSingleton<IRepository, MongoRepository>();

builder.Services.AddSingleton<IRandomizer, BuiltInRandomizer>();

builder.Services.Configure<PokerStoveEvaluatorOptions>(builder.Configuration.GetSection("PokerStoveEvaluator"));
builder.Services.AddSingleton<IEvaluator, PokerStoveEvaluator>();

var host = builder.Build();
host.Run();
