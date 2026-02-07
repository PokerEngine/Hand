using Domain.ValueObject;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Infrastructure.Client.MongoDb;

public class MongoDbClient
{
    public MongoClient Client;
    public MongoDbClient(IOptions<MongoDbClientOptions> options)
    {
        var url = $"mongodb://{options.Value.Username}:{options.Value.Password}@{options.Value.Host}:{options.Value.Port}";
        Client = new MongoClient(url);

        MongoDbSerializerConfig.Register();
    }
}

public class MongoDbClientOptions
{
    public const string SectionName = "MongoDb";

    public required string Host { get; init; }
    public required int Port { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}

internal static class MongoDbSerializerConfig
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
    private const string MaxSeatField = "maxSeat";
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
        int maxSeat = default;
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

                case MaxSeatField:
                    maxSeat = context.Reader.ReadInt32();
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
            MaxSeat = maxSeat,
            SmallBlind = smallBlind,
            BigBlind = bigBlind
        };
    }
}

internal sealed class PositionsSerializer : SerializerBase<Positions>
{
    private const string SmallBlindSeatField = "smallBlindSeat";
    private const string BigBlindSeatField = "bigBlindSeat";
    private const string ButtonSeatField = "buttonSeat";

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Positions value)
    {
        context.Writer.WriteStartDocument();
        context.Writer.WriteName(SmallBlindSeatField);
        context.Writer.WriteInt32(value.SmallBlindSeat);
        context.Writer.WriteName(BigBlindSeatField);
        context.Writer.WriteInt32(value.BigBlindSeat);
        context.Writer.WriteName(ButtonSeatField);
        context.Writer.WriteInt32(value.ButtonSeat);
        context.Writer.WriteEndDocument();
    }

    public override Positions Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        int smallBlindSeat = default;
        int bigBlindSeat = default;
        int buttonSeat = default;

        context.Reader.ReadStartDocument();

        while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var name = context.Reader.ReadName(Utf8NameDecoder.Instance);

            switch (name)
            {
                case SmallBlindSeatField:
                    smallBlindSeat = context.Reader.ReadInt32();
                    break;

                case BigBlindSeatField:
                    bigBlindSeat = context.Reader.ReadInt32();
                    break;

                case ButtonSeatField:
                    buttonSeat = context.Reader.ReadInt32();
                    break;

                default:
                    context.Reader.SkipValue();
                    break;
            }
        }

        context.Reader.ReadEndDocument();

        return new Positions
        {
            SmallBlindSeat = smallBlindSeat,
            BigBlindSeat = bigBlindSeat,
            ButtonSeat = buttonSeat
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
