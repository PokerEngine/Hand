using Application.Command;
using Application.Event;
using Application.IntegrationEvent;
using Application.Query;
using Application.Repository;
using Application.Storage;
using Domain.Event;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Infrastructure.Client.MongoDb;
using Infrastructure.Client.RabbitMq;
using Infrastructure.Command;
using Infrastructure.Event;
using Infrastructure.IntegrationEvent;
using Infrastructure.Query;
using Infrastructure.Repository;
using Infrastructure.Service.Evaluator;
using Infrastructure.Service.Randomizer;
using Infrastructure.Storage;

namespace Infrastructure;

public static class Bootstrapper
{
    public static WebApplicationBuilder PrepareApplicationBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddEnvironmentVariables();
        builder.Services.AddOpenApi();

        // Register clients
        builder.Services.Configure<MongoDbClientOptions>(
            builder.Configuration.GetSection(MongoDbClientOptions.SectionName)
        );
        builder.Services.AddSingleton<MongoDbClient>();
        builder.Services.Configure<RabbitMqClientOptions>(
            builder.Configuration.GetSection(RabbitMqClientOptions.SectionName)
        );
        builder.Services.AddSingleton<RabbitMqClient>();

        // Register services
        builder.Services.AddSingleton<IRandomizer, BuiltInRandomizer>();
        builder.Services.Configure<PokerStoveEvaluatorOptions>(builder.Configuration.GetSection(PokerStoveEvaluatorOptions.SectionName));
        builder.Services.AddSingleton<IEvaluator, PokerStoveEvaluator>();

        // Register repository
        builder.Services.Configure<MongoDbRepositoryOptions>(
            builder.Configuration.GetSection(MongoDbRepositoryOptions.SectionName)
        );
        builder.Services.AddSingleton<IRepository, MongoDbRepository>();

        // Register storage
        builder.Services.Configure<MongoDbStorageOptions>(
            builder.Configuration.GetSection(MongoDbStorageOptions.SectionName)
        );
        builder.Services.AddSingleton<IStorage, MongoDbStorage>();

        // Register commands
        RegisterCommandHandler<StartHandCommand, StartHandHandler, StartHandResponse>(builder.Services);
        RegisterCommandHandler<SubmitPlayerActionCommand, SubmitPlayerActionHandler, SubmitPlayerActionResponse>(builder.Services);
        builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();

        // Register queries
        RegisterQueryHandler<GetHandDetailQuery, GetHandDetailHandler, GetHandDetailResponse>(builder.Services);
        builder.Services.AddScoped<IQueryDispatcher, QueryDispatcher>();

        // Register domain events
        RegisterEventHandler<HandStartedEvent, HandStartedEventHandler>(builder.Services);
        RegisterEventHandler<HandFinishedEvent, HandFinishedEventHandler>(builder.Services);
        RegisterEventHandler<SmallBlindPostedEvent, SmallBlindPostedEventHandler>(builder.Services);
        RegisterEventHandler<BigBlindPostedEvent, BigBlindPostedEventHandler>(builder.Services);
        RegisterEventHandler<HoleCardsDealtEvent, HoleCardsDealtEventHandler>(builder.Services);
        RegisterEventHandler<BoardCardsDealtEvent, BoardCardsDealtEventHandler>(builder.Services);
        RegisterEventHandler<PlayerActionRequestedEvent, PlayerActionRequestedEventHandler>(builder.Services);
        RegisterEventHandler<PlayerActedEvent, PlayerActedEventHandler>(builder.Services);
        RegisterEventHandler<BetRefundedEvent, BetRefundedEventHandler>(builder.Services);
        RegisterEventHandler<SidePotAwardedEvent, SidePotAwardedEventHandler>(builder.Services);
        RegisterEventHandler<HoleCardsShownEvent, HoleCardsShownEventHandler>(builder.Services);
        RegisterEventHandler<HoleCardsMuckedEvent, HoleCardsMuckedEventHandler>(builder.Services);
        builder.Services.AddScoped<IEventDispatcher, EventDispatcher>();

        // Register integration events
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
