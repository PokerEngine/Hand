using Application.Exception;
using Application.Repository;
using Domain.Event;
using Domain.ValueObject;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Infrastructure.Repository;

public class MongoDbRepository : IRepository
{
    private readonly IMongoCollection<EventDocument> _collection;
    private const string Collection = "events";

    public MongoDbRepository(IOptions<MongoDbRepositoryOptions> options, ILogger<MongoDbRepository> logger)
    {
        var url = $"mongodb://{options.Value.Username}:{options.Value.Password}@{options.Value.Host}:{options.Value.Port}";
        var client = new MongoClient(url);
        var db = client.GetDatabase(options.Value.Database);
        _collection = db.GetCollection<EventDocument>(Collection);

        BsonSerializerConfig.Register();
    }

    public Task<HandUid> GetNextUidAsync()
    {
        return Task.FromResult(new HandUid(Guid.NewGuid()));
    }

    public async Task<List<IEvent>> GetEventsAsync(HandUid handUid)
    {
        var documents = await _collection
            .Find(e => e.HandUid == handUid)
            .SortBy(e => e.Id)
            .ToListAsync();

        var events = new List<IEvent>();

        foreach (var document in documents)
        {
            var type = MongoDbEventTypeResolver.GetType(document.Type);
            var @event = (IEvent)BsonSerializer.Deserialize(document.Data, type);
            events.Add(@event);
        }

        if (events.Count == 0)
        {
            throw new HandNotFoundException("The hand is not found");
        }

        return events;
    }

    public async Task AddEventsAsync(HandUid handUid, List<IEvent> events)
    {
        var documents = events.Select(e => new EventDocument
        {
            Type = MongoDbEventTypeResolver.GetName(e),
            HandUid = handUid,
            OccurredAt = e.OccurredAt,
            Data = e.ToBsonDocument(e.GetType())
        });

        await _collection.InsertManyAsync(documents);
    }
}

public class MongoDbRepositoryOptions
{
    public const string SectionName = "MongoDB";

    public required string Host { get; init; }
    public required int Port { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string Database { get; init; }
}

internal sealed class EventDocument
{
    [BsonId]
    public ObjectId Id { get; init; }

    public required string Type { get; init; }
    public required HandUid HandUid { get; init; }
    public required DateTime OccurredAt { get; init; }
    public required BsonDocument Data { get; init; }
}


internal static class BsonSerializerConfig
{
    public static void Register()
    {
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        BsonSerializer.TryRegisterSerializer(new HandUidSerializer());
        BsonSerializer.TryRegisterSerializer(new TableUidSerializer());
        BsonSerializer.TryRegisterSerializer(new NicknameSerializer());
        BsonSerializer.TryRegisterSerializer(new SeatSerializer());
        BsonSerializer.TryRegisterSerializer(new ChipsSerializer());
        BsonSerializer.TryRegisterSerializer(new CardSetSerializer());
        BsonSerializer.TryRegisterSerializer(new SidePotSerializer());
        BsonSerializer.TryRegisterSerializer(new RulesSerializer());
        BsonSerializer.TryRegisterSerializer(new PositionsSerializer());
        BsonSerializer.TryRegisterSerializer(new PlayerActionSerializer());
        BsonSerializer.TryRegisterSerializer(new ComboSerializer());
        BsonSerializer.TryRegisterSerializer(new ParticipantSerializer());
    }
}

internal sealed class HandUidSerializer : SerializerBase<HandUid>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, HandUid value)
        => context.Writer.WriteGuid(value);

    public override HandUid Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        => context.Reader.ReadGuid();
}

internal sealed class TableUidSerializer : SerializerBase<TableUid>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TableUid value)
        => context.Writer.WriteGuid(value);

    public override TableUid Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        => context.Reader.ReadGuid();
}

internal sealed class NicknameSerializer : SerializerBase<Nickname>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Nickname value)
        => context.Writer.WriteString(value);

    public override Nickname Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        => context.Reader.ReadString();
}

internal sealed class SeatSerializer : SerializerBase<Seat>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Seat value)
        => context.Writer.WriteInt32(value);

    public override Seat Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        => context.Reader.ReadInt32();
}

internal sealed class ChipsSerializer : SerializerBase<Chips>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Chips value)
        => context.Writer.WriteInt32(value);

    public override Chips Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        => context.Reader.ReadInt32();
}

internal sealed class CardSetSerializer : SerializerBase<CardSet>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, CardSet value)
        => context.Writer.WriteString(value);

    public override CardSet Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        => context.Reader.ReadString();
}

internal sealed class SidePotSerializer : SerializerBase<SidePot>
{
    private const string CompetitorsField = "competitors";
    private const string BetsField = "bets";
    private const string AnteField = "ante";

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, SidePot value)
    {
        context.Writer.WriteStartDocument();

        context.Writer.WriteName(CompetitorsField);
        context.Writer.WriteStartArray();
        foreach (var nickname in value.Competitors)
        {
            context.Writer.WriteString(nickname);
            // BsonSerializer.Serialize(context.Writer, nickname);
        }
        context.Writer.WriteEndArray();

        context.Writer.WriteName(BetsField);
        context.Writer.WriteStartDocument();
        foreach (var (nickname, amount) in value.Bets)
        {
            context.Writer.WriteName(nickname);
            context.Writer.WriteInt32(amount);
        }
        context.Writer.WriteEndDocument();

        context.Writer.WriteName(AnteField);
        context.Writer.WriteInt32(value.Ante);
        context.Writer.WriteEndDocument();
    }

    public override SidePot Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        HashSet<Nickname> competitors = new();
        Bets bets = new();
        Chips ante = new();

        context.Reader.ReadStartDocument();

        while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var name = context.Reader.ReadName(Utf8NameDecoder.Instance);

            switch (name)
            {
                case CompetitorsField:
                    context.Reader.ReadStartArray();
                    while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
                    {
                        var nickname = context.Reader.ReadString();
                        competitors.Add(nickname);
                    }
                    context.Reader.ReadEndArray();
                    break;

                case BetsField:
                    context.Reader.ReadStartDocument();
                    while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
                    {
                        var nickname = context.Reader.ReadName(Utf8NameDecoder.Instance);
                        var amount = context.Reader.ReadInt32();
                        bets = bets.Post(nickname, amount);
                    }
                    context.Reader.ReadEndDocument();
                    break;

                case AnteField:
                    ante = context.Reader.ReadInt32();
                    break;

                default:
                    context.Reader.SkipValue();
                    break;
            }
        }

        context.Reader.ReadEndDocument();

        return new SidePot(competitors, bets, ante);
    }
}

internal sealed class RulesSerializer : SerializerBase<Rules>
{
    private const string GameField = "game";
    private const string SmallBlindField = "smallBlind";
    private const string BigBlindField = "bigBlind";

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Rules value)
    {
        context.Writer.WriteStartDocument();
        context.Writer.WriteName(GameField);
        context.Writer.WriteString(value.Game.ToString());
        context.Writer.WriteName(SmallBlindField);
        context.Writer.WriteInt32(value.SmallBlind);
        context.Writer.WriteName(BigBlindField);
        context.Writer.WriteInt32(value.BigBlind);
        context.Writer.WriteEndDocument();
    }

    public override Rules Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        Game game = default;
        int smallBlind = default;
        int bigBlind = default;

        context.Reader.ReadStartDocument();

        while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var name = context.Reader.ReadName(Utf8NameDecoder.Instance);

            switch (name)
            {
                case GameField:
                    game = Enum.Parse<Game>(
                        context.Reader.ReadString(),
                        ignoreCase: true
                    );
                    break;

                case SmallBlindField:
                    smallBlind = context.Reader.ReadInt32();
                    break;

                case BigBlindField:
                    bigBlind = context.Reader.ReadInt32();
                    break;

                default:
                    context.Reader.SkipValue();
                    break;
            }
        }

        context.Reader.ReadEndDocument();

        return new Rules
        {
            Game = game,
            SmallBlind = smallBlind,
            BigBlind = bigBlind
        };
    }
}

internal sealed class PositionsSerializer : SerializerBase<Positions>
{
    private const string SmallBlindField = "smallBlind";
    private const string BigBlindField = "bigBlind";
    private const string ButtonField = "button";
    private const string MaxField = "max";

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Positions value)
    {
        context.Writer.WriteStartDocument();
        context.Writer.WriteName(SmallBlindField);
        context.Writer.WriteInt32(value.SmallBlind);
        context.Writer.WriteName(BigBlindField);
        context.Writer.WriteInt32(value.BigBlind);
        context.Writer.WriteName(ButtonField);
        context.Writer.WriteInt32(value.Button);
        context.Writer.WriteName(MaxField);
        context.Writer.WriteInt32(value.Max);
        context.Writer.WriteEndDocument();
    }

    public override Positions Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        int smallBlind = default;
        int bigBlind = default;
        int button = default;
        int max = default;

        context.Reader.ReadStartDocument();

        while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var name = context.Reader.ReadName(Utf8NameDecoder.Instance);

            switch (name)
            {
                case SmallBlindField:
                    smallBlind = context.Reader.ReadInt32();
                    break;

                case BigBlindField:
                    bigBlind = context.Reader.ReadInt32();
                    break;

                case ButtonField:
                    button = context.Reader.ReadInt32();
                    break;

                case MaxField:
                    max = context.Reader.ReadInt32();
                    break;

                default:
                    context.Reader.SkipValue();
                    break;
            }
        }

        context.Reader.ReadEndDocument();

        return new Positions
        {
            SmallBlind = smallBlind,
            BigBlind = bigBlind,
            Button = button,
            Max = max
        };
    }
}

internal sealed class PlayerActionSerializer : SerializerBase<PlayerAction>
{
    private const string TypeField = "type";
    private const string AmountField = "amount";

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, PlayerAction value)
    {
        context.Writer.WriteStartDocument();
        context.Writer.WriteName(TypeField);
        context.Writer.WriteString(value.Type.ToString());
        context.Writer.WriteName(AmountField);
        context.Writer.WriteInt32(value.Amount);
        context.Writer.WriteEndDocument();
    }

    public override PlayerAction Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        PlayerActionType type = default;
        Chips amount = default;

        context.Reader.ReadStartDocument();

        while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var name = context.Reader.ReadName(Utf8NameDecoder.Instance);

            switch (name)
            {
                case TypeField:
                    type = Enum.Parse<PlayerActionType>(context.Reader.ReadString());
                    break;

                case AmountField:
                    amount = context.Reader.ReadInt32();
                    break;

                default:
                    context.Reader.SkipValue();
                    break;
            }
        }

        context.Reader.ReadEndDocument();

        return new PlayerAction(type, amount);
    }
}

internal sealed class ComboSerializer : SerializerBase<Combo>
{
    private const string TypeField = "type";
    private const string WeightField = "weight";

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Combo value)
    {
        context.Writer.WriteStartDocument();
        context.Writer.WriteName(TypeField);
        context.Writer.WriteString(value.Type.ToString());
        context.Writer.WriteName(WeightField);
        context.Writer.WriteInt32(value.Weight);
        context.Writer.WriteEndDocument();
    }

    public override Combo Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        ComboType type = default;
        int weight = default;

        context.Reader.ReadStartDocument();

        while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var name = context.Reader.ReadName(Utf8NameDecoder.Instance);

            switch (name)
            {
                case TypeField:
                    type = Enum.Parse<ComboType>(context.Reader.ReadString());
                    break;

                case WeightField:
                    weight = context.Reader.ReadInt32();
                    break;

                default:
                    context.Reader.SkipValue();
                    break;
            }
        }

        context.Reader.ReadEndDocument();

        return new Combo(type, weight);
    }
}

internal sealed class ParticipantSerializer : SerializerBase<Participant>
{
    private const string NicknameField = "nickname";
    private const string SeatField = "seat";
    private const string StackField = "stack";

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Participant value)
    {
        context.Writer.WriteStartDocument();
        context.Writer.WriteName(NicknameField);
        context.Writer.WriteString(value.Nickname);
        context.Writer.WriteName(SeatField);
        context.Writer.WriteInt32(value.Seat);
        context.Writer.WriteName(StackField);
        context.Writer.WriteInt32(value.Stack);
        context.Writer.WriteEndDocument();
    }

    public override Participant Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        Nickname nickname = default;
        Seat seat = default;
        Chips stack = default;

        context.Reader.ReadStartDocument();

        while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var name = context.Reader.ReadName(Utf8NameDecoder.Instance);

            switch (name)
            {
                case NicknameField:
                    nickname = context.Reader.ReadString();
                    break;

                case SeatField:
                    seat = context.Reader.ReadInt32();
                    break;

                case StackField:
                    stack = context.Reader.ReadInt32();
                    break;

                default:
                    context.Reader.SkipValue();
                    break;
            }
        }

        context.Reader.ReadEndDocument();

        return new Participant
        {
            Nickname = nickname,
            Seat = seat,
            Stack = stack
        };
    }
}
