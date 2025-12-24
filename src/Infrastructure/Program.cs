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

builder.Services.Configure<RabbitMqIntegrationEventBusOptions>(builder.Configuration.GetSection(RabbitMqIntegrationEventBusOptions.SectionName));
builder.Services.AddSingleton<IIntegrationEventBus, RabbitMqIntegrationEventBus>();

builder.Services.Configure<MongoDbRepositoryOptions>(builder.Configuration.GetSection(MongoDbRepositoryOptions.SectionName));
builder.Services.AddSingleton<IRepository, MongoDbRepository>();

builder.Services.AddSingleton<IRandomizer, BuiltInRandomizer>();

builder.Services.Configure<PokerStoveEvaluatorOptions>(builder.Configuration.GetSection(PokerStoveEvaluatorOptions.SectionName));
builder.Services.AddSingleton<IEvaluator, PokerStoveEvaluator>();

var host = builder.Build();
host.Run();
