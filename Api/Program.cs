using Api.Authentication;
using Application.Authentication;
using Application.Command;
using Application.Event;
using Application.IntegrationEvent;
using Application.Query;
using Application.Repository;
using Application.Storage;
using Application.UnitOfWork;
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
using Microsoft.AspNetCore.Authentication;

namespace Api;

public static class Bootstrapper
{
    public static WebApplicationBuilder PrepareApplicationBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddEnvironmentVariables();

        AddDomainServices(builder);
        AddPersistence(builder);
        AddDomainEvents(builder);
        AddIntegrationEvents(builder);
        AddCommands(builder);
        AddQueries(builder);
        AddAuthentication(builder);
        AddControllers(builder);

        return builder;
    }

    private static void AddDomainServices(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IRandomizer, BuiltInRandomizer>();

        builder.Services.Configure<PokerStoveEvaluatorOptions>(builder.Configuration.GetSection(PokerStoveEvaluatorOptions.SectionName));
        builder.Services.AddSingleton<IEvaluator, PokerStoveEvaluator>();
    }

    private static void AddPersistence(WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddSingleton<IHandRepository, InMemoryHandRepository>();
            builder.Services.AddSingleton<IHandStorage, InMemoryHandStorage>();
        }
        else
        {
            builder.Services.Configure<MongoDbClientOptions>(
                builder.Configuration.GetSection(MongoDbClientOptions.SectionName)
            );
            builder.Services.AddSingleton<MongoDbClient>();

            builder.Services.Configure<MongoDbRepositoryOptions>(
                builder.Configuration.GetSection(MongoDbRepositoryOptions.SectionName)
            );
            builder.Services.AddSingleton<IHandRepository, MongoDbHandRepository>();

            builder.Services.Configure<MongoDbStorageOptions>(
                builder.Configuration.GetSection(MongoDbStorageOptions.SectionName)
            );
            builder.Services.AddSingleton<IHandStorage, MongoDbHandStorage>();
        }
    }

    private static void AddDomainEvents(WebApplicationBuilder builder)
    {
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
    }

    private static void AddIntegrationEvents(WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddSingleton<IIntegrationEventPublisher, InMemoryIntegrationEventPublisher>();
        }
        else
        {
            builder.Services.Configure<RabbitMqClientOptions>(
                builder.Configuration.GetSection(RabbitMqClientOptions.SectionName)
            );
            builder.Services.AddSingleton<RabbitMqClient>();

            builder.Services.Configure<RabbitMqIntegrationEventPublisherOptions>(
                builder.Configuration.GetSection(RabbitMqIntegrationEventPublisherOptions.SectionName)
            );
            builder.Services.AddSingleton<IIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();
        }
    }

    private static void AddCommands(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        RegisterCommandHandler<StartHandCommand, StartHandHandler, StartHandResponse>(builder.Services);
        RegisterCommandHandler<SubmitPlayerActionCommand, SubmitPlayerActionHandler, SubmitPlayerActionResponse>(builder.Services);
        builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();
    }

    private static void AddQueries(WebApplicationBuilder builder)
    {
        RegisterQueryHandler<GetHandDetailQuery, GetHandDetailHandler, GetHandDetailResponse>(builder.Services);
        builder.Services.AddScoped<IQueryDispatcher, QueryDispatcher>();
    }

    private static void AddControllers(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
    }

    private static void AddAuthentication(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserProvider, HttpContextCurrentUserProvider>();

        if (builder.Environment.IsDevelopment())
        {
            var authentication = builder.Services.AddAuthentication(DevelopmentAuthenticationHandler.SchemeName);
            authentication.AddScheme<AuthenticationSchemeOptions, DevelopmentAuthenticationHandler>(DevelopmentAuthenticationHandler.SchemeName, null);
        }
        else
        {
            var authentication = builder.Services.AddAuthentication(JwtAuthenticationHandler.SchemeName);
            builder.Services.Configure<JwtAuthenticationOptions>(
                builder.Configuration.GetSection(JwtAuthenticationOptions.SectionName)
            );
            authentication.AddScheme<AuthenticationSchemeOptions, JwtAuthenticationHandler>(JwtAuthenticationHandler.SchemeName, null);
        }

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("HasNickname", p => p.RequireClaim("nickname"));
        });
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

    private static WebApplication CreateWebApplication(string[] args)
    {
        var builder = Bootstrapper.PrepareApplicationBuilder(args);
        return ConfigureApplication(builder);
    }

    private static WebApplication ConfigureApplication(WebApplicationBuilder builder)
    {
        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapOpenApi();

        return app;
    }
}
