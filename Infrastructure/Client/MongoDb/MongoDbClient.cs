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

internal sealed class GameSerializer : SerializerBase<Game>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Game value)
        => context.Writer.WriteString(value.ToString());

    public override Game Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        => Enum.Parse<Game>(context.Reader.ReadString(), ignoreCase: true);
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
            BsonSerializer.Serialize(context.Writer, nickname);
        }
        context.Writer.WriteEndArray();

        context.Writer.WriteName(BetsField);
        context.Writer.WriteStartDocument();
        foreach (var (nickname, amount) in value.Bets)
        {
            context.Writer.WriteName(nickname);
            BsonSerializer.Serialize(context.Writer, amount);
        }
        context.Writer.WriteEndDocument();

        context.Writer.WriteName(AnteField);
        BsonSerializer.Serialize(context.Writer, value.Ante);

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
                        competitors.Add(BsonSerializer.Deserialize<Nickname>(context.Reader));
                    }
                    context.Reader.ReadEndArray();
                    break;

                case BetsField:
                    context.Reader.ReadStartDocument();
                    while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
                    {
                        var nickname = context.Reader.ReadName(Utf8NameDecoder.Instance);
                        var amount = BsonSerializer.Deserialize<Chips>(context.Reader);
                        bets = bets.Post(nickname, amount);
                    }
                    context.Reader.ReadEndDocument();
                    break;

                case AnteField:
                    ante = BsonSerializer.Deserialize<Chips>(context.Reader);
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
        BsonSerializer.Serialize(context.Writer, value.Game);
        context.Writer.WriteName(MaxSeatField);
        BsonSerializer.Serialize(context.Writer, value.MaxSeat);
        context.Writer.WriteName(SmallBlindField);
        BsonSerializer.Serialize(context.Writer, value.SmallBlind);
        context.Writer.WriteName(BigBlindField);
        BsonSerializer.Serialize(context.Writer, value.BigBlind);
        context.Writer.WriteEndDocument();
    }

    public override Rules Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        Game game = default;
        Seat maxSeat = default;
        Chips smallBlind = default;
        Chips bigBlind = default;

        context.Reader.ReadStartDocument();

        while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var name = context.Reader.ReadName(Utf8NameDecoder.Instance);

            switch (name)
            {
                case GameField:
                    game = BsonSerializer.Deserialize<Game>(context.Reader);
                    break;

                case MaxSeatField:
                    maxSeat = BsonSerializer.Deserialize<Seat>(context.Reader);
                    break;

                case SmallBlindField:
                    smallBlind = BsonSerializer.Deserialize<Chips>(context.Reader);
                    break;

                case BigBlindField:
                    bigBlind = BsonSerializer.Deserialize<Chips>(context.Reader);
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
        BsonSerializer.Serialize(context.Writer, value.SmallBlindSeat);
        context.Writer.WriteName(BigBlindSeatField);
        BsonSerializer.Serialize(context.Writer, value.BigBlindSeat);
        context.Writer.WriteName(ButtonSeatField);
        BsonSerializer.Serialize(context.Writer, value.ButtonSeat);
        context.Writer.WriteEndDocument();
    }

    public override Positions Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        Seat smallBlindSeat = default;
        Seat bigBlindSeat = default;
        Seat buttonSeat = default;

        context.Reader.ReadStartDocument();

        while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var name = context.Reader.ReadName(Utf8NameDecoder.Instance);

            switch (name)
            {
                case SmallBlindSeatField:
                    smallBlindSeat = BsonSerializer.Deserialize<Seat>(context.Reader);
                    break;

                case BigBlindSeatField:
                    bigBlindSeat = BsonSerializer.Deserialize<Seat>(context.Reader);
                    break;

                case ButtonSeatField:
                    buttonSeat = BsonSerializer.Deserialize<Seat>(context.Reader);
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
        BsonSerializer.Serialize(context.Writer, value.Amount);
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
                    amount = BsonSerializer.Deserialize<Chips>(context.Reader);
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
        BsonSerializer.Serialize(context.Writer, value.Nickname);
        context.Writer.WriteName(SeatField);
        BsonSerializer.Serialize(context.Writer, value.Seat);
        context.Writer.WriteName(StackField);
        BsonSerializer.Serialize(context.Writer, value.Stack);
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
                    nickname = BsonSerializer.Deserialize<Nickname>(context.Reader);
                    break;

                case SeatField:
                    seat = BsonSerializer.Deserialize<Seat>(context.Reader);
                    break;

                case StackField:
                    stack = BsonSerializer.Deserialize<Chips>(context.Reader);
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
