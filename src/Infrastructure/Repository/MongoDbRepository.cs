using Application.Repository;
using Domain;
using Domain.Event;
using Domain.ValueObject;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Collections.Immutable;

namespace Infrastructure.Repository;

public class MongoDbRepository : IRepository
{
    private readonly ILogger<MongoDbRepository> _logger;
    private readonly IMongoCollection<BaseDocument> _collection;
    private readonly EventDocumentMapper _mapper;
    private bool _isConnected;
    private const string Collection = "events";

    public MongoDbRepository(IConfiguration configuration, ILogger<MongoDbRepository> logger)
    {
        var host = configuration.GetValue<string>("MongoDB:Host") ??
                   throw new ArgumentException("MongoDB:Host is not configured", nameof(configuration));
        var port = configuration.GetValue<int?>("MongoDB:Port") ??
                   throw new ArgumentException("MongoDB:Port is not configured", nameof(configuration));
        var username = configuration.GetValue<string>("MongoDB:Username") ??
                       throw new ArgumentException("MongoDB:Username is not configured", nameof(configuration));
        var password = configuration.GetValue<string>("MongoDB:Password") ??
                       throw new ArgumentException("MongoDB:Password is not configured", nameof(configuration));
        var database = configuration.GetValue<string>("MongoDB:Database") ??
                       throw new ArgumentException("MongoDB:Database is not configured", nameof(configuration));

        _logger = logger;

        var url = $"mongodb://{username}:{password}@{host}:{port}";
        var client = new MongoClient(url);
        var db = client.GetDatabase(database);
        _collection = db.GetCollection<BaseDocument>(Collection);

        _mapper = new EventDocumentMapper();

        RegisterClassMap();
    }

    public async Task Connect()
    {
        if (_isConnected)
        {
            throw new InvalidOperationException("Already connected");
        }

        _isConnected = true;
        await Task.CompletedTask;

        _logger.LogInformation("Connected");
    }

    public async Task Disconnect()
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("Not connected");
        }

        _isConnected = false;
        await Task.CompletedTask;

        _logger.LogInformation("Disconnected");
    }

    public async Task<IList<BaseEvent>> GetEvents(HandUid handUid)
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("Not connected");
        }

        var sort = Builders<BaseDocument>.Sort.Ascending("_id");
        var findOptions = new FindOptions<BaseDocument>
        {
            Sort = sort
        };
        var cursor = await _collection.FindAsync(e => e.HandUid == handUid, findOptions);
        var documents = await cursor.ToListAsync();

        var events = new List<BaseEvent>();
        foreach (var document in documents)
        {
            var @event = _mapper.ToEvent((dynamic)document);
            events.Add(@event);
        }

        _logger.LogInformation("{eventCount} events are got for {handUid}", events.Count, handUid);
        return events;
    }

    public async Task AddEvents(HandUid handUid, IList<BaseEvent> events)
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("Not connected");
        }

        var documents = new List<BaseDocument>();
        foreach (var @event in events)
        {
            var document = _mapper.ToDocument((dynamic)@event, handUid);
            documents.Add(document);
        }

        await _collection.InsertManyAsync(documents);

        _logger.LogInformation("{eventCount} events are added for {handUid}", events.Count, handUid);
    }

    private static void RegisterClassMap()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(BaseDocument)))
        {
            BsonClassMap.RegisterClassMap<BaseDocument>(cm =>
            {
                cm.AutoMap();
                cm.SetIsRootClass(true);
                cm.AddKnownType(typeof(HandIsCreatedDocument));
                cm.AddKnownType(typeof(HandIsStartedDocument));
                cm.AddKnownType(typeof(HandIsFinishedDocument));
                cm.AddKnownType(typeof(StageIsStartedDocument));
                cm.AddKnownType(typeof(StageIsFinishedDocument));
                cm.AddKnownType(typeof(PlayerConnectedDocument));
                cm.AddKnownType(typeof(PlayerDisconnectedDocument));
                cm.AddKnownType(typeof(SmallBlindIsPostedDocument));
                cm.AddKnownType(typeof(BigBlindIsPostedDocument));
                cm.AddKnownType(typeof(HoleCardsAreDealtDocument));
                cm.AddKnownType(typeof(BoardCardsAreDealtDocument));
                cm.AddKnownType(typeof(RefundIsCommittedDocument));
                cm.AddKnownType(typeof(WinWithoutShowdownIsCommittedDocument));
                cm.AddKnownType(typeof(WinAtShowdownIsCommittedDocument));
                cm.AddKnownType(typeof(HoleCardsAreMuckedDocument));
                cm.AddKnownType(typeof(HoleCardsAreShownDocument));
            });

            BsonClassMap.RegisterClassMap<HandIsCreatedDocument>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<HandIsStartedDocument>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<HandIsFinishedDocument>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<StageIsStartedDocument>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<StageIsFinishedDocument>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<PlayerConnectedDocument>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<PlayerDisconnectedDocument>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<SmallBlindIsPostedDocument>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<BigBlindIsPostedDocument>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<HoleCardsAreDealtDocument>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<BoardCardsAreDealtDocument>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<RefundIsCommittedDocument>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<WinWithoutShowdownIsCommittedDocument>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<WinAtShowdownIsCommittedDocument>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<HoleCardsAreMuckedDocument>(cm => cm.AutoMap());
            BsonClassMap.RegisterClassMap<HoleCardsAreShownDocument>(cm => cm.AutoMap());
        }
    }
}

internal abstract record BaseDocument
{
    public required DateTime OccuredAt { get; init; }
    public required HandUid HandUid { get; init; }
}

[BsonIgnoreExtraElements]
internal record HandIsCreatedDocument : BaseDocument
{
    public required Game Game { get; init; }
    public required int SmallBlind { get; init; }
    public required int BigBlind { get; init; }
    public required int SmallBlindSeat { get; init; }
    public required int BigBlindSeat { get; init; }
    public required int ButtonSeat { get; init; }
    public required ImmutableList<(string, int, int)> Participants { get; init; }
}

[BsonIgnoreExtraElements]
internal record HandIsStartedDocument : BaseDocument;

[BsonIgnoreExtraElements]
internal record HandIsFinishedDocument : BaseDocument;

[BsonIgnoreExtraElements]
internal record StageIsStartedDocument : BaseDocument;

[BsonIgnoreExtraElements]
internal record StageIsFinishedDocument : BaseDocument;

[BsonIgnoreExtraElements]
internal record PlayerConnectedDocument : BaseDocument
{
    public required string Nickname { get; init; }
}

[BsonIgnoreExtraElements]
internal record PlayerDisconnectedDocument : BaseDocument
{
    public required string Nickname { get; init; }
}

[BsonIgnoreExtraElements]
internal record SmallBlindIsPostedDocument : BaseDocument
{
    public required string Nickname { get; init; }
    public required int Amount { get; init; }
}

[BsonIgnoreExtraElements]
internal record BigBlindIsPostedDocument : BaseDocument
{
    public required string Nickname { get; init; }
    public required int Amount { get; init; }
}

[BsonIgnoreExtraElements]
internal record HoleCardsAreDealtDocument : BaseDocument
{
    public required string Nickname { get; init; }
    public required string Cards { get; init; }
}

[BsonIgnoreExtraElements]
internal record BoardCardsAreDealtDocument : BaseDocument
{
    public required string Cards { get; init; }
}

[BsonIgnoreExtraElements]
internal record DecisionIsRequestedDocument : BaseDocument
{
    public required string Nickname { get; init; }
    public required bool FoldIsAvailable { get; init; }
    public required bool CheckIsAvailable { get; init; }
    public required bool CallIsAvailable { get; init; }
    public required int CallToAmount { get; init; }
    public required bool RaiseIsAvailable { get; init; }
    public required int MinRaiseToAmount { get; init; }
    public required int MaxRaiseToAmount { get; init; }
}

[BsonIgnoreExtraElements]
internal record DecisionIsCommittedDocument : BaseDocument
{
    public required string Nickname { get; init; }
    public required DecisionType DecisionType { get; init; }
    public required int DecisionAmount { get; init; }
}

[BsonIgnoreExtraElements]
internal record RefundIsCommittedDocument : BaseDocument
{
    public required string Nickname { get; init; }
    public required int Amount { get; init; }
}

[BsonIgnoreExtraElements]
internal record WinWithoutShowdownIsCommittedDocument : BaseDocument
{
    public required string Nickname { get; init; }
    public required int Amount { get; init; }
}

[BsonIgnoreExtraElements]
internal record WinAtShowdownIsCommittedDocument : BaseDocument
{
    public required ImmutableDictionary<string, int> SidePot { get; init; }
    public required ImmutableDictionary<string, int> WinPot { get; init; }
}

[BsonIgnoreExtraElements]
internal record HoleCardsAreShownDocument : BaseDocument
{
    public required string Nickname { get; init; }
    public required string Cards { get; init; }
    public required ComboType ComboType { get; init; }
    public required int ComboWeight { get; init; }
}

[BsonIgnoreExtraElements]
internal record HoleCardsAreMuckedDocument : BaseDocument
{
    public required string Nickname { get; init; }
}

internal class EventDocumentMapper
{
    public HandIsCreatedDocument ToDocument(HandIsCreatedEvent @event, HandUid handUid)
    {
        return new HandIsCreatedDocument
        {
            Game = @event.Game,
            SmallBlind = @event.SmallBlind,
            BigBlind = @event.BigBlind,
            SmallBlindSeat = @event.SmallBlindSeat,
            BigBlindSeat = @event.BigBlindSeat,
            ButtonSeat = @event.ButtonSeat,
            Participants = @event.Participants.Select(x => ((string)x.Nickname, (int)x.Seat, (int)x.Stake)).ToImmutableList(),
            OccuredAt = @event.OccuredAt,
            HandUid = handUid
        };
    }

    public HandIsCreatedEvent ToEvent(HandIsCreatedDocument document)
    {
        List<Participant> participants = [];

        foreach (var (nickname, seat, stake) in document.Participants)
        {
            participants.Add(new Participant(nickname, seat, stake));
        }

        return new HandIsCreatedEvent(
            Game: document.Game,
            SmallBlind: document.SmallBlind,
            BigBlind: document.BigBlind,
            SmallBlindSeat: document.SmallBlindSeat,
            BigBlindSeat: document.BigBlindSeat,
            ButtonSeat: document.ButtonSeat,
            Participants: participants.ToImmutableList(),
            OccuredAt: document.OccuredAt
        );
    }

    public HandIsStartedDocument ToDocument(HandIsStartedEvent @event, HandUid handUid)
    {
        return new HandIsStartedDocument
        {
            OccuredAt = @event.OccuredAt,
            HandUid = handUid
        };
    }

    public HandIsStartedEvent ToEvent(HandIsStartedDocument document)
    {
        return new HandIsStartedEvent(
            OccuredAt: document.OccuredAt
        );
    }

    public HandIsFinishedDocument ToDocument(HandIsFinishedEvent @event, HandUid handUid)
    {
        return new HandIsFinishedDocument
        {
            OccuredAt = @event.OccuredAt,
            HandUid = handUid
        };
    }

    public HandIsFinishedEvent ToEvent(HandIsFinishedDocument document)
    {
        return new HandIsFinishedEvent(
            OccuredAt: document.OccuredAt
        );
    }

    public StageIsStartedDocument ToDocument(StageIsStartedEvent @event, HandUid handUid)
    {
        return new StageIsStartedDocument
        {
            OccuredAt = @event.OccuredAt,
            HandUid = handUid
        };
    }

    public StageIsStartedEvent ToEvent(StageIsStartedDocument document)
    {
        return new StageIsStartedEvent(
            OccuredAt: document.OccuredAt
        );
    }

    public StageIsFinishedDocument ToDocument(StageIsFinishedEvent @event, HandUid handUid)
    {
        return new StageIsFinishedDocument
        {
            OccuredAt = @event.OccuredAt,
            HandUid = handUid
        };
    }

    public StageIsFinishedEvent ToEvent(StageIsFinishedDocument document)
    {
        return new StageIsFinishedEvent(
            OccuredAt: document.OccuredAt
        );
    }

    public PlayerConnectedDocument ToDocument(PlayerConnectedEvent @event, HandUid handUid)
    {
        return new PlayerConnectedDocument
        {
            Nickname = @event.Nickname,
            OccuredAt = @event.OccuredAt,
            HandUid = handUid
        };
    }

    public PlayerConnectedEvent ToEvent(PlayerConnectedDocument document)
    {
        return new PlayerConnectedEvent(
            Nickname: document.Nickname,
            OccuredAt: document.OccuredAt
        );
    }

    public PlayerDisconnectedDocument ToDocument(PlayerDisconnectedEvent @event, HandUid handUid)
    {
        return new PlayerDisconnectedDocument
        {
            Nickname = @event.Nickname,
            OccuredAt = @event.OccuredAt,
            HandUid = handUid
        };
    }

    public PlayerDisconnectedEvent ToEvent(PlayerDisconnectedDocument document)
    {
        return new PlayerDisconnectedEvent(
            Nickname: document.Nickname,
            OccuredAt: document.OccuredAt
        );
    }

    public SmallBlindIsPostedDocument ToDocument(SmallBlindIsPostedEvent @event, HandUid handUid)
    {
        return new SmallBlindIsPostedDocument
        {
            Nickname = @event.Nickname,
            Amount = @event.Amount,
            OccuredAt = @event.OccuredAt,
            HandUid = handUid
        };
    }

    public SmallBlindIsPostedEvent ToEvent(SmallBlindIsPostedDocument document)
    {
        return new SmallBlindIsPostedEvent(
            Nickname: document.Nickname,
            Amount: document.Amount,
            OccuredAt: document.OccuredAt
        );
    }

    public BigBlindIsPostedDocument ToDocument(BigBlindIsPostedEvent @event, HandUid handUid)
    {
        return new BigBlindIsPostedDocument
        {
            Nickname = @event.Nickname,
            Amount = @event.Amount,
            OccuredAt = @event.OccuredAt,
            HandUid = handUid
        };
    }

    public BigBlindIsPostedEvent ToEvent(BigBlindIsPostedDocument document)
    {
        return new BigBlindIsPostedEvent(
            Nickname: document.Nickname,
            Amount: document.Amount,
            OccuredAt: document.OccuredAt
        );
    }

    public HoleCardsAreDealtDocument ToDocument(HoleCardsAreDealtEvent @event, HandUid handUid)
    {
        return new HoleCardsAreDealtDocument
        {
            Nickname = @event.Nickname,
            Cards = @event.Cards.ToString(),
            OccuredAt = @event.OccuredAt,
            HandUid = handUid
        };
    }

    public HoleCardsAreDealtEvent ToEvent(HoleCardsAreDealtDocument document)
    {
        return new HoleCardsAreDealtEvent(
            Nickname: document.Nickname,
            Cards: CardSet.FromString(document.Cards),
            OccuredAt: document.OccuredAt
        );
    }

    public BoardCardsAreDealtDocument ToDocument(BoardCardsAreDealtEvent @event, HandUid handUid)
    {
        return new BoardCardsAreDealtDocument
        {
            Cards = @event.Cards.ToString(),
            OccuredAt = @event.OccuredAt,
            HandUid = handUid
        };
    }

    public BoardCardsAreDealtEvent ToEvent(BoardCardsAreDealtDocument document)
    {
        return new BoardCardsAreDealtEvent(
            Cards: CardSet.FromString(document.Cards),
            OccuredAt: document.OccuredAt
        );
    }

    public DecisionIsRequestedDocument ToDocument(DecisionIsRequestedEvent @event, HandUid handUid)
    {
        return new DecisionIsRequestedDocument
        {
            Nickname = @event.Nickname,
            FoldIsAvailable = @event.FoldIsAvailable,
            CheckIsAvailable = @event.CheckIsAvailable,
            CallIsAvailable = @event.CallIsAvailable,
            CallToAmount = @event.CallToAmount,
            RaiseIsAvailable = @event.RaiseIsAvailable,
            MinRaiseToAmount = @event.MinRaiseToAmount,
            MaxRaiseToAmount = @event.MaxRaiseToAmount,
            OccuredAt = @event.OccuredAt,
            HandUid = handUid
        };
    }

    public DecisionIsRequestedEvent ToEvent(DecisionIsRequestedDocument document)
    {
        return new DecisionIsRequestedEvent(
            Nickname: document.Nickname,
            FoldIsAvailable: document.FoldIsAvailable,
            CheckIsAvailable: document.CheckIsAvailable,
            CallIsAvailable: document.CallIsAvailable,
            CallToAmount: document.CallToAmount,
            RaiseIsAvailable: document.RaiseIsAvailable,
            MinRaiseToAmount: document.MinRaiseToAmount,
            MaxRaiseToAmount: document.MaxRaiseToAmount,
            OccuredAt: document.OccuredAt
        );
    }

    public DecisionIsCommittedDocument ToDocument(DecisionIsCommittedEvent @event, HandUid handUid)
    {
        return new DecisionIsCommittedDocument
        {
            Nickname = @event.Nickname,
            DecisionType = @event.Decision.Type,
            DecisionAmount = @event.Decision.Amount,
            OccuredAt = @event.OccuredAt,
            HandUid = handUid
        };
    }

    public DecisionIsCommittedEvent ToEvent(DecisionIsCommittedDocument document)
    {
        return new DecisionIsCommittedEvent(
            Nickname: document.Nickname,
            Decision: new Decision(document.DecisionType, document.DecisionAmount),
            OccuredAt: document.OccuredAt
        );
    }

    public RefundIsCommittedDocument ToDocument(RefundIsCommittedEvent @event, HandUid handUid)
    {
        return new RefundIsCommittedDocument
        {
            Nickname = @event.Nickname,
            Amount = @event.Amount,
            OccuredAt = @event.OccuredAt,
            HandUid = handUid
        };
    }

    public RefundIsCommittedEvent ToEvent(RefundIsCommittedDocument document)
    {
        return new RefundIsCommittedEvent(
            Nickname: document.Nickname,
            Amount: document.Amount,
            OccuredAt: document.OccuredAt
        );
    }

    public WinWithoutShowdownIsCommittedDocument ToDocument(WinWithoutShowdownIsCommittedEvent @event, HandUid handUid)
    {
        return new WinWithoutShowdownIsCommittedDocument
        {
            Nickname = @event.Nickname,
            Amount = @event.Amount,
            OccuredAt = @event.OccuredAt,
            HandUid = handUid
        };
    }

    public WinWithoutShowdownIsCommittedEvent ToEvent(WinWithoutShowdownIsCommittedDocument document)
    {
        return new WinWithoutShowdownIsCommittedEvent(
            Nickname: document.Nickname,
            Amount: document.Amount,
            OccuredAt: document.OccuredAt
        );
    }

    public WinAtShowdownIsCommittedDocument ToDocument(WinAtShowdownIsCommittedEvent @event, HandUid handUid)
    {
        return new WinAtShowdownIsCommittedDocument
        {
            SidePot = @event.SidePot.ToImmutableDictionary(x => (string)x.Key, x => (int)x.Value),
            WinPot = @event.WinPot.ToImmutableDictionary(x => (string)x.Key, x => (int)x.Value),
            OccuredAt = @event.OccuredAt,
            HandUid = handUid
        };
    }

    public WinAtShowdownIsCommittedEvent ToEvent(WinAtShowdownIsCommittedDocument document)
    {
        return new WinAtShowdownIsCommittedEvent(
            SidePot: new SidePot(document.SidePot.ToDictionary(x => (Nickname)x.Key, x => (Chips)x.Value)),
            WinPot: new SidePot(document.WinPot.ToDictionary(x => (Nickname)x.Key, x => (Chips)x.Value)),
            OccuredAt: document.OccuredAt
        );
    }

    public HoleCardsAreMuckedDocument ToDocument(HoleCardsAreMuckedEvent @event, HandUid handUid)
    {
        return new HoleCardsAreMuckedDocument
        {
            Nickname = @event.Nickname,
            OccuredAt = @event.OccuredAt,
            HandUid = handUid
        };
    }

    public HoleCardsAreMuckedEvent ToEvent(HoleCardsAreMuckedDocument document)
    {
        return new HoleCardsAreMuckedEvent(
            Nickname: document.Nickname,
            OccuredAt: document.OccuredAt
        );
    }

    public HoleCardsAreShownDocument ToDocument(HoleCardsAreShownEvent @event, HandUid handUid)
    {
        return new HoleCardsAreShownDocument
        {
            Nickname = @event.Nickname,
            Cards = @event.Cards.ToString(),
            ComboType = @event.Combo.Type,
            ComboWeight = @event.Combo.Weight,
            OccuredAt = @event.OccuredAt,
            HandUid = handUid
        };
    }

    public HoleCardsAreShownEvent ToEvent(HoleCardsAreShownDocument document)
    {
        return new HoleCardsAreShownEvent(
            Nickname: document.Nickname,
            Cards: CardSet.FromString(document.Cards),
            Combo: new Combo(document.ComboType, document.ComboWeight),
            OccuredAt: document.OccuredAt
        );
    }

    public BaseDocument ToDocument(BaseEvent @event, HandUid handUid)
    {
        throw new NotImplementedException($"Mapper is not implemented for {@event.GetType().Name}");
    }

    public BaseDocument ToEvent(BaseDocument document)
    {
        throw new NotImplementedException($"Mapper is not implemented for {document.GetType().Name}");
    }
}
