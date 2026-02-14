using Application.Exception;
using Application.Storage;
using Domain.Entity;
using Domain.ValueObject;
using System.Collections.Concurrent;

namespace Application.Test.Storage;

public class StubStorage : IStorage
{
    private readonly ConcurrentDictionary<HandUid, DetailView> _detailMapping = new();

    public Task<DetailView> GetDetailViewAsync(HandUid handUid)
    {
        if (!_detailMapping.TryGetValue(handUid, out var view))
        {
            throw new HandNotFoundException("The hand is not found");
        }

        return Task.FromResult(view);
    }

    public Task SaveViewAsync(Hand hand)
    {
        var state = hand.GetState();

        var view = new DetailView
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
        _detailMapping.AddOrUpdate(hand.Uid, view, (_, _) => view);
        return Task.CompletedTask;
    }
}
