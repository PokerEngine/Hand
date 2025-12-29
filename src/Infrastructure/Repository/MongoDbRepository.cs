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
            var type = Type.GetType(document.Type, throwOnError: true)!;
            var @event = (IEvent)BsonSerializer.Deserialize(document.Data, type);
            events.Add(@event);
        }

        if (events.Count == 0)
        {
            throw new InvalidOperationException("The hand is not found");
        }

        return events;
    }

    public async Task AddEventsAsync(HandUid handUid, List<IEvent> events)
    {
        var documents = events.Select(e => new EventDocument
        {
            Type = e.GetType().AssemblyQualifiedName!,
            HandUid = handUid,
            OccurredAt = e.OccuredAt,
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
        BsonSerializer.TryRegisterSerializer(new NicknameSerializer());
        BsonSerializer.TryRegisterSerializer(new SeatSerializer());
        BsonSerializer.TryRegisterSerializer(new ChipsSerializer());
        BsonSerializer.TryRegisterSerializer(new CardSetSerializer());
        BsonSerializer.TryRegisterSerializer(new DecisionSerializer());
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
        => context.Writer.WriteString(value.ToString());

    public override CardSet Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        => CardSet.FromString(context.Reader.ReadString());
}

internal sealed class DecisionSerializer : SerializerBase<Decision>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Decision value)
    {
        context.Writer.WriteStartDocument();
        context.Writer.WriteName("type");
        context.Writer.WriteString(value.Type.ToString());
        context.Writer.WriteName("amount");
        context.Writer.WriteInt32(value.Amount);
        context.Writer.WriteEndDocument();
    }

    public override Decision Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        DecisionType type = default;
        Chips amount = default;

        context.Reader.ReadStartDocument();

        while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var name = context.Reader.ReadName(Utf8NameDecoder.Instance);

            switch (name)
            {
                case "type":
                    type = Enum.Parse<DecisionType>(
                        context.Reader.ReadString(),
                        ignoreCase: true
                    );
                    break;

                case "amount":
                    amount = context.Reader.ReadInt32();
                    break;

                default:
                    context.Reader.SkipValue();
                    break;
            }
        }

        context.Reader.ReadEndDocument();

        return new Decision(type, amount);
    }
}

internal sealed class ComboSerializer : SerializerBase<Combo>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Combo value)
    {
        context.Writer.WriteStartDocument();
        context.Writer.WriteName("type");
        context.Writer.WriteString(value.Type.ToString());
        context.Writer.WriteName("weight");
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
                case "type":
                    type = Enum.Parse<ComboType>(
                        context.Reader.ReadString(),
                        ignoreCase: true
                    );
                    break;

                case "weight":
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
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Participant value)
    {
        context.Writer.WriteStartDocument();
        context.Writer.WriteName("nickname");
        context.Writer.WriteString(value.Nickname);
        context.Writer.WriteName("seat");
        context.Writer.WriteInt32(value.Seat);
        context.Writer.WriteName("stack");
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
                case "nickname":
                    nickname = context.Reader.ReadString();
                    break;

                case "seat":
                    seat = context.Reader.ReadInt32();
                    break;

                case "stack":
                    stack = context.Reader.ReadInt32();
                    break;

                default:
                    context.Reader.SkipValue();
                    break;
            }
        }

        context.Reader.ReadEndDocument();

        return new Participant(nickname, seat, stack);
    }
}
