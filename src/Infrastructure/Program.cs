using Application.Command;
using Application.Event;
using Application.IntegrationEvent;
using Application.Query;
using Application.Repository;
using Domain.Event;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Infrastructure.Command;
using Infrastructure.Event;
using Infrastructure.IntegrationEvent;
using Infrastructure.Query;
using Infrastructure.Repository;
using Infrastructure.Service.Evaluator;
using Infrastructure.Service.Randomizer;

namespace Infrastructure;

public static class Bootstrapper
{
    public static WebApplicationBuilder PrepareApplicationBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddEnvironmentVariables();
        builder.Services.AddOpenApi();

        // Register services
        builder.Services.AddSingleton<IRandomizer, BuiltInRandomizer>();
        builder.Services.Configure<PokerStoveEvaluatorOptions>(builder.Configuration.GetSection(PokerStoveEvaluatorOptions.SectionName));
        builder.Services.AddSingleton<IEvaluator, PokerStoveEvaluator>();

        // Register repository
        builder.Services.Configure<MongoDbRepositoryOptions>(
            builder.Configuration.GetSection(MongoDbRepositoryOptions.SectionName)
        );
        builder.Services.AddSingleton<IRepository, MongoDbRepository>();

        // Register commands
        RegisterCommandHandler<CreateHandCommand, CreateHandHandler, CreateHandResponse>(builder.Services);
        RegisterCommandHandler<StartHandCommand, StartHandHandler, StartHandResponse>(builder.Services);
        RegisterCommandHandler<CommitDecisionCommand, CommitDecisionHandler, CommitDecisionResponse>(builder.Services);
        builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();

        // Register queries
        RegisterQueryHandler<GetHandByUidQuery, GetHandByUidHandler, GetHandByUidResponse>(builder.Services);
        builder.Services.AddScoped<IQueryDispatcher, QueryDispatcher>();

        // Register domain events
        RegisterEventHandler<HandIsCreatedEvent, HandIsCreatedEventHandler>(builder.Services);
        RegisterEventHandler<HandIsStartedEvent, HandIsStartedEventHandler>(builder.Services);
        RegisterEventHandler<HandIsFinishedEvent, HandIsFinishedEventHandler>(builder.Services);
        RegisterEventHandler<SmallBlindIsPostedEvent, SmallBlindIsPostedEventHandler>(builder.Services);
        RegisterEventHandler<BigBlindIsPostedEvent, BigBlindIsPostedEventHandler>(builder.Services);
        RegisterEventHandler<HoleCardsAreDealtEvent, HoleCardsAreDealtEventHandler>(builder.Services);
        RegisterEventHandler<BoardCardsAreDealtEvent, BoardCardsAreDealtEventHandler>(builder.Services);
        RegisterEventHandler<DecisionIsRequestedEvent, DecisionIsRequestedEventHandler>(builder.Services);
        RegisterEventHandler<DecisionIsCommittedEvent, DecisionIsCommittedEventHandler>(builder.Services);
        RegisterEventHandler<RefundIsCommittedEvent, RefundIsCommittedEventHandler>(builder.Services);
        RegisterEventHandler<WinAtShowdownIsCommittedEvent, WinAtShowdownIsCommittedEventHandler>(builder.Services);
        RegisterEventHandler<WinWithoutShowdownIsCommittedEvent, WinWithoutShowdownIsCommittedEventHandler>(builder.Services);
        RegisterEventHandler<HoleCardsAreShownEvent, HoleCardsAreShownEventHandler>(builder.Services);
        RegisterEventHandler<HoleCardsAreMuckedEvent, HoleCardsAreMuckedEventHandler>(builder.Services);
        builder.Services.AddScoped<IEventDispatcher, EventDispatcher>();

        // Register integration events
        builder.Services.Configure<RabbitMqConnectionOptions>(
            builder.Configuration.GetSection(RabbitMqConnectionOptions.SectionName)
        );
        builder.Services.Configure<RabbitMqIntegrationEventPublisherOptions>(
            builder.Configuration.GetSection(RabbitMqIntegrationEventPublisherOptions.SectionName)
        );
        builder.Services.AddScoped<IIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        return builder;
    }

    private static void RegisterCommandHandler<TCommand, THandler, TResponse>(IServiceCollection services)
        where TCommand : ICommand
        where TResponse : ICommandResponse
        where THandler : class, ICommandHandler<TCommand, TResponse>
    {
        services.AddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand, TResponse>>(provider => provider.GetRequiredService<THandler>());
    }

    private static void RegisterQueryHandler<TQuery, THandler, TResponse>(IServiceCollection services)
        where TQuery : IQuery
        where TResponse : IQueryResponse
        where THandler : class, IQueryHandler<TQuery, TResponse>
    {
        services.AddScoped<THandler>();
        services.AddScoped<IQueryHandler<TQuery, TResponse>>(provider => provider.GetRequiredService<THandler>());
    }

    private static void RegisterEventHandler<TEvent, THandler>(IServiceCollection services)
        where TEvent : IEvent
        where THandler : class, IEventHandler<TEvent>
    {
        services.AddScoped<THandler>();
        services.AddScoped<IEventHandler<TEvent>>(provider => provider.GetRequiredService<THandler>());
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var app = CreateWebApplication(args);
        app.Run();
    }

    // Public method for creating the WebApplication - can be called by tests
    // This allows WebApplicationFactory to work properly with the minimal hosting model
    private static WebApplication CreateWebApplication(string[] args)
    {
        var builder = Bootstrapper.PrepareApplicationBuilder(args);
        return ConfigureApplication(builder);
    }

    // Configure the application pipeline
    private static WebApplication ConfigureApplication(WebApplicationBuilder builder)
    {
        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseWebSockets();

        app.MapOpenApi();
        app.MapControllers();

        return app;
    }
}
