using Application.Exception;
using Application.Storage;
using Domain.Entity;
using Domain.ValueObject;
using Infrastructure.Client.MongoDb;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Infrastructure.Storage;

public class MongoDbStorage : IStorage
{
    private const string DetailViewCollectionName = "views_detail";
    private readonly IMongoCollection<DetailViewDocument> _detailViewCollection;

    public MongoDbStorage(MongoDbClient client, IOptions<MongoDbStorageOptions> options)
    {
        var db = client.Client.GetDatabase(options.Value.Database);

        _detailViewCollection = db.GetCollection<DetailViewDocument>(DetailViewCollectionName);
    }

    public async Task<DetailView> GetDetailViewAsync(HandUid uid)
    {
        var document = await _detailViewCollection
            .Find(x => x.Uid == (Guid)uid)
            .FirstOrDefaultAsync();

        if (document is null)
        {
            throw new HandNotFoundException("The hand is not found");
        }

        return new DetailView
        {
            Uid = document.Uid,
            TableUid = document.TableUid,
            TableType = document.TableType,
            Rules = document.Rules,
            Table = document.Table,
            Pot = document.Pot
        };
    }

    public async Task SaveViewAsync(Hand hand)
    {
        var options = new FindOneAndReplaceOptions<DetailViewDocument>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        var state = hand.GetState();

        var document = new DetailViewDocument
        {
            Uid = hand.Uid,
            TableUid = hand.TableUid,
            TableType = hand.TableType.ToString(),
            Rules = new DetailViewRules
            {
                Game = state.Rules.Game.ToString(),
                SmallBlind = state.Rules.SmallBlind,
                BigBlind = state.Rules.BigBlind,
                MaxSeat = state.Rules.MaxSeat
            },
            Table = new DetailViewTable
            {
                Positions = new DetailViewPositions
                {
                    BigBlindSeat = state.Table.Positions.BigBlindSeat,
                    SmallBlindSeat = state.Table.Positions.SmallBlindSeat,
                    ButtonSeat = state.Table.Positions.ButtonSeat
                },
                Players = state.Table.Players.Select(p => new DetailViewPlayer
                {
                    Nickname = p.Nickname,
                    Seat = p.Seat,
                    Stack = p.Stack,
                    HoleCards = p.HoleCards,
                    IsFolded = p.IsFolded
                }).ToList(),
                BoardCards = state.Table.BoardCards
            },
            Pot = new DetailViewPot
            {
                Ante = state.Pot.Ante,
                CollectedBets = state.Pot.CollectedBets.Select(x => new DetailViewBet
                {
                    Nickname = x.Nickname,
                    Amount = x.Amount
                }).ToList(),
                CurrentBets = state.Pot.CurrentBets.Select(x => new DetailViewBet
                {
                    Nickname = x.Nickname,
                    Amount = x.Amount
                }).ToList(),
                Awards = state.Pot.Awards.Select(x => new DetailViewAward
                {
                    Winners = x.Winners.Select(y => y.ToString()).ToList(),
                    Amount = x.Amount
                }).ToList()
            }
        };

        await _detailViewCollection.FindOneAndReplaceAsync(x => x.Uid == (Guid)hand.Uid, document, options);
    }
}

public class MongoDbStorageOptions
{
    public const string SectionName = "MongoDbStorage";

    public required string Database { get; init; }
}

public record DetailViewDocument
{
    [BsonId]
    public required Guid Uid { get; init; }
    public required Guid TableUid { get; init; }
    public required string TableType { get; init; }
    public required DetailViewRules Rules { get; init; }
    public required DetailViewTable Table { get; init; }
    public required DetailViewPot Pot { get; init; }
}
