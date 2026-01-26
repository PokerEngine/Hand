using Domain.Entity;
using Domain.Event;
using Domain.Test.Service.Evaluator;
using Domain.Test.Service.Randomizer;
using Domain.ValueObject;

namespace Domain.Test.Entity;

public class NoLimitHoldem6MaxHandTest
{
    private readonly StubRandomizer _randomizer = new();
    private readonly StubEvaluator _evaluator = new();

    [Fact]
    public void TestFromScratch()
    {
        var handUid = new HandUid(Guid.NewGuid());
        var tableUid = new TableUid(Guid.NewGuid());
        var participantSb = new Participant
        {
            Nickname = new Nickname("SmallBlind"),
            Seat = new Seat(1),
            Stack = new Chips(1000)
        };
        var participantBb = new Participant
        {
            Nickname = new Nickname("BigBlind"),
            Seat = new Seat(2),
            Stack = new Chips(900)
        };
        var participantEp = new Participant
        {
            Nickname = new Nickname("Early"),
            Seat = new Seat(3),
            Stack = new Chips(800)
        };
        var participantMp = new Participant
        {
            Nickname = new Nickname("Middle"),
            Seat = new Seat(4),
            Stack = new Chips(700)
        };
        var participantCo = new Participant
        {
            Nickname = new Nickname("CutOff"),
            Seat = new Seat(5),
            Stack = new Chips(600)
        };
        var participantBu = new Participant
        {
            Nickname = new Nickname("Button"),
            Seat = new Seat(6),
            Stack = new Chips(500)
        };

        var hand = Hand.FromScratch(
            uid: handUid,
            tableUid: tableUid,
            tableType: TableType.Cash,
            rules: new()
            {
                Game = Game.NoLimitHoldem,
                SmallBlind = new Chips(5),
                BigBlind = new Chips(10)
            },
            positions: new()
            {
                SmallBlind = new Seat(1),
                BigBlind = new Seat(2),
                Button = new Seat(6),
                Max = new Seat(6)
            },
            participants: [participantSb, participantBb, participantEp, participantMp, participantCo, participantBu],
            randomizer: _randomizer,
            evaluator: _evaluator
        );

        Assert.Equal(handUid, hand.Uid);
        Assert.Equal(tableUid, hand.TableUid);
        Assert.Equal(TableType.Cash, hand.TableType);
        Assert.Equal(Game.NoLimitHoldem, hand.Rules.Game);
        Assert.Equal(new Chips(5), hand.Rules.SmallBlind);
        Assert.Equal(new Chips(10), hand.Rules.BigBlind);
        Assert.Equal(new Seat(1), hand.Table.Positions.SmallBlind);
        Assert.Equal(new Seat(2), hand.Table.Positions.BigBlind);
        Assert.Equal(new Seat(6), hand.Table.Positions.Button);
        Assert.Equal(new Seat(6), hand.Table.Positions.Max);
        Assert.Equal(new Chips(0), hand.Pot.TotalAmount);
        Assert.IsType<StandardDeck>(hand.Deck);

        var events = hand.PullEvents();
        Assert.Single(events);
        var @event = Assert.IsType<HandCreatedEvent>(events[0]);
        Assert.Equal(Game.NoLimitHoldem, @event.Rules.Game);
        Assert.Equal(new Chips(5), @event.Rules.SmallBlind);
        Assert.Equal(new Chips(10), @event.Rules.BigBlind);
        Assert.Equal(6, @event.Participants.Count);
        Assert.Equal(participantSb, @event.Participants[0]);
        Assert.Equal(participantBb, @event.Participants[1]);
        Assert.Equal(participantEp, @event.Participants[2]);
        Assert.Equal(participantMp, @event.Participants[3]);
        Assert.Equal(participantCo, @event.Participants[4]);
        Assert.Equal(participantBu, @event.Participants[5]);
    }

    [Fact]
    public void TestFromEvents()
    {
        var handUid = new HandUid(Guid.NewGuid());
        var tableUid = new TableUid(Guid.NewGuid());
        var participantSb = new Participant
        {
            Nickname = new Nickname("SmallBlind"),
            Seat = new Seat(1),
            Stack = new Chips(1000)
        };
        var participantBb = new Participant
        {
            Nickname = new Nickname("BigBlind"),
            Seat = new Seat(2),
            Stack = new Chips(900)
        };
        var participantBu = new Participant
        {
            Nickname = new Nickname("Button"),
            Seat = new Seat(6),
            Stack = new Chips(800)
        };

        var events = new List<IEvent>
        {
            new HandCreatedEvent
            {
                TableUid = tableUid,
                TableType = TableType.Cash,
                Rules = new ()
                {
                    Game = Game.NoLimitHoldem,
                    SmallBlind = new Chips(5),
                    BigBlind = new Chips(10)
                },
                Positions = new ()
                {
                    SmallBlind = new Seat(1),
                    BigBlind = new Seat(2),
                    Button = new Seat(6),
                    Max = new Seat(6)
                },
                Participants = [participantSb, participantBb, participantBu],
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new HandStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new SmallBlindPostedEvent
            {
                Nickname = participantSb.Nickname,
                Amount = new Chips(5),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new BigBlindPostedEvent
            {
                Nickname = participantBb.Nickname,
                Amount = new Chips(10),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new HoleCardsDealtEvent
            {
                Nickname = participantSb.Nickname,
                Cards = new CardSet([Card.TreyOfClubs, Card.NineOfClubs]),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new HoleCardsDealtEvent
            {
                Nickname = participantBb.Nickname,
                Cards = new CardSet([Card.QueenOfClubs, Card.TenOfDiamonds]),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new HoleCardsDealtEvent
            {
                Nickname = participantBu.Nickname,
                Cards = new CardSet([Card.SevenOfSpades, Card.EightOfSpades]),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new PlayerActionRequestedEvent
            {
                Nickname = participantBu.Nickname,
                FoldIsAvailable = true,
                CheckIsAvailable = false,
                CallIsAvailable = true,
                CallByAmount = new Chips(10),
                RaiseIsAvailable = true,
                MinRaiseByAmount = new Chips(20),
                MaxRaiseByAmount = new Chips(800),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new PlayerActedEvent
            {
                Nickname = participantBu.Nickname,
                Action = new PlayerAction(PlayerActionType.RaiseBy, new Chips(25)),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new PlayerActionRequestedEvent
            {
                Nickname = participantSb.Nickname,
                FoldIsAvailable = true,
                CheckIsAvailable = false,
                CallIsAvailable = true,
                CallByAmount = new Chips(20),
                RaiseIsAvailable = true,
                MinRaiseByAmount = new Chips(35),
                MaxRaiseByAmount = new Chips(1000),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new PlayerActedEvent
            {
                Nickname = participantSb.Nickname,
                Action = new PlayerAction(PlayerActionType.Fold),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new PlayerActionRequestedEvent
            {
                Nickname = participantBb.Nickname,
                FoldIsAvailable = true,
                CheckIsAvailable = false,
                CallIsAvailable = true,
                CallByAmount = new Chips(15),
                RaiseIsAvailable = true,
                MinRaiseByAmount = new Chips(30),
                MaxRaiseByAmount = new Chips(900),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new PlayerActedEvent
            {
                Nickname = participantBb.Nickname,
                Action = new PlayerAction(PlayerActionType.CallBy, new Chips(15)),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new BetsCollectedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new BoardCardsDealtEvent
            {
                Cards = new CardSet([Card.AceOfSpades, Card.SevenOfClubs, Card.DeuceOfDiamonds]),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new PlayerActionRequestedEvent
            {
                Nickname = participantBb.Nickname,
                FoldIsAvailable = false,
                CheckIsAvailable = true,
                CallIsAvailable = false,
                CallByAmount = new Chips(0),
                RaiseIsAvailable = true,
                MinRaiseByAmount = new Chips(10),
                MaxRaiseByAmount = new Chips(875),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new PlayerActedEvent
            {
                Nickname = participantBb.Nickname,
                Action = new PlayerAction(PlayerActionType.Check),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new PlayerActionRequestedEvent
            {
                Nickname = participantBu.Nickname,
                FoldIsAvailable = false,
                CheckIsAvailable = true,
                CallIsAvailable = false,
                CallByAmount = new Chips(0),
                RaiseIsAvailable = true,
                MinRaiseByAmount = new Chips(10),
                MaxRaiseByAmount = new Chips(775),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new PlayerActedEvent
            {
                Nickname = participantBu.Nickname,
                Action = new PlayerAction(PlayerActionType.RaiseBy, new Chips(15)),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new PlayerActionRequestedEvent
            {
                Nickname = participantBb.Nickname,
                FoldIsAvailable = true,
                CheckIsAvailable = false,
                CallIsAvailable = true,
                CallByAmount = new Chips(15),
                RaiseIsAvailable = true,
                MinRaiseByAmount = new Chips(30),
                MaxRaiseByAmount = new Chips(875),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new PlayerActedEvent
            {
                Nickname = participantBb.Nickname,
                Action = new PlayerAction(PlayerActionType.Fold),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new BetRefundedEvent
            {
                Nickname = participantBu.Nickname,
                Amount = new Chips(15),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new HoleCardsMuckedEvent
            {
                Nickname = participantBu.Nickname,
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new SidePotAwardedEvent
            {
                Winners = [participantBu.Nickname],
                SidePot = new SidePot(
                    [participantBu.Nickname],
                    new Bets()
                        .Post(participantSb.Nickname, new Chips(5))
                        .Post(participantBb.Nickname, new Chips(25))
                        .Post(participantBu.Nickname, new Chips(25)),
                    new Chips(0)
                ),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new HandFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            }
        };

        var hand = Hand.FromEvents(
            uid: handUid,
            randomizer: _randomizer,
            evaluator: _evaluator,
            events: events
        );

        Assert.Equal(handUid, hand.Uid);
        Assert.Equal(tableUid, hand.TableUid);
        Assert.Equal(Game.NoLimitHoldem, hand.Rules.Game);
        Assert.Equal(new Chips(5), hand.Rules.SmallBlind);
        Assert.Equal(new Chips(10), hand.Rules.BigBlind);
        Assert.Equal(new Seat(1), hand.Table.Positions.SmallBlind);
        Assert.Equal(new Seat(2), hand.Table.Positions.BigBlind);
        Assert.Equal(new Seat(6), hand.Table.Positions.Button);
        Assert.Equal(new Seat(6), hand.Table.Positions.Max);
        Assert.Equal(new Chips(0), hand.Pot.TotalAmount);
        Assert.IsType<StandardDeck>(hand.Deck);

        Assert.Equal(new CardSet([Card.AceOfSpades, Card.SevenOfClubs, Card.DeuceOfDiamonds]), hand.Table.BoardCards);

        var playerSb = hand.Table.GetPlayerByNickname(participantSb.Nickname);
        var playerBb = hand.Table.GetPlayerByNickname(participantBb.Nickname);
        var playerBu = hand.Table.GetPlayerByNickname(participantBu.Nickname);

        Assert.Equal(new CardSet([Card.TreyOfClubs, Card.NineOfClubs]), playerSb.HoleCards);
        Assert.Equal(new Chips(995), playerSb.Stack);
        Assert.True(playerSb.IsFolded);

        Assert.Equal(new CardSet([Card.QueenOfClubs, Card.TenOfDiamonds]), playerBb.HoleCards);
        Assert.Equal(new Chips(875), playerBb.Stack);
        Assert.True(playerBb.IsFolded);

        Assert.Equal(new CardSet([Card.SevenOfSpades, Card.EightOfSpades]), playerBu.HoleCards);
        Assert.Equal(new Chips(775), playerBu.Stack);
        Assert.False(playerBu.IsFolded);
    }

    [Fact]
    public void TestGetState()
    {
        var participantSb = CreateParticipant(
            nickname: "SmallBlind",
            seat: 6
        );
        var participantBb = CreateParticipant(
            nickname: "BigBlind",
            seat: 2
        );
        var participantBu = CreateParticipant(
            nickname: "Button",
            seat: 4
        );

        var hand = CreateHand(
            participants: [participantSb, participantBb, participantBu],
            smallBlindSeat: 6,
            bigBlindSeat: 2,
            buttonSeat: 4
        );
        hand.Start();
        hand.SubmitPlayerAction(
            nickname: new Nickname("Button"),
            action: new PlayerAction(PlayerActionType.RaiseBy, new Chips(25))
        );
        hand.SubmitPlayerAction(
            nickname: new Nickname("SmallBlind"),
            action: new PlayerAction(PlayerActionType.Fold)
        );
        hand.SubmitPlayerAction(
            nickname: new Nickname("BigBlind"),
            action: new PlayerAction(PlayerActionType.CallBy, new Chips(15))
        );
        hand.SubmitPlayerAction(
            nickname: new Nickname("BigBlind"),
            action: new PlayerAction(PlayerActionType.Check)
        );
        hand.SubmitPlayerAction(
            nickname: new Nickname("Button"),
            action: new PlayerAction(PlayerActionType.RaiseBy, new Chips(15))
        );

        var state = hand.GetState();

        Assert.Equal(3, state.Table.BoardCards.Count);
        Assert.Equal(new Seat(6), state.Table.Positions.SmallBlind);
        Assert.Equal(new Seat(2), state.Table.Positions.BigBlind);
        Assert.Equal(new Seat(4), state.Table.Positions.Button);
        Assert.Equal(new Seat(6), state.Table.Positions.Max);
        Assert.Equal(3, state.Table.Players.Count);
        Assert.Equal(new Nickname("BigBlind"), state.Table.Players[0].Nickname);
        Assert.Equal(new Chips(975), state.Table.Players[0].Stack);
        Assert.Equal(2, state.Table.Players[0].HoleCards.Count);
        Assert.False(state.Table.Players[0].IsFolded);
        Assert.Equal(new Nickname("Button"), state.Table.Players[1].Nickname);
        Assert.Equal(new Chips(960), state.Table.Players[1].Stack);
        Assert.Equal(2, state.Table.Players[1].HoleCards.Count);
        Assert.False(state.Table.Players[1].IsFolded);
        Assert.Equal(new Nickname("SmallBlind"), state.Table.Players[2].Nickname);
        Assert.Equal(new Chips(995), state.Table.Players[2].Stack);
        Assert.Equal(2, state.Table.Players[2].HoleCards.Count);
        Assert.True(state.Table.Players[2].IsFolded);

        Assert.Equal(new Chips(0), state.Pot.Ante);
        Assert.Equal(3, state.Pot.CollectedBets.Count);
        Assert.Equal(new Nickname("SmallBlind"), state.Pot.CollectedBets[0].Nickname);
        Assert.Equal(new Chips(5), state.Pot.CollectedBets[0].Amount);
        Assert.Equal(new Nickname("BigBlind"), state.Pot.CollectedBets[1].Nickname);
        Assert.Equal(new Chips(25), state.Pot.CollectedBets[1].Amount);
        Assert.Equal(new Nickname("Button"), state.Pot.CollectedBets[2].Nickname);
        Assert.Equal(new Chips(25), state.Pot.CollectedBets[2].Amount);
        Assert.Equal(2, state.Pot.CurrentBets.Count);
        Assert.Equal(new Nickname("BigBlind"), state.Pot.CurrentBets[0].Nickname);
        Assert.Equal(new Chips(0), state.Pot.CurrentBets[0].Amount);
        Assert.Equal(new Nickname("Button"), state.Pot.CurrentBets[1].Nickname);
        Assert.Equal(new Chips(15), state.Pot.CurrentBets[1].Amount);
        Assert.Empty(state.Pot.Awards);
    }

    [Fact]
    public void TestFlow()
    {
        var participantSb = CreateParticipant(
            nickname: "SmallBlind",
            seat: 6
        );
        var participantBb = CreateParticipant(
            nickname: "BigBlind",
            seat: 2
        );
        var participantBu = CreateParticipant(
            nickname: "Button",
            seat: 4
        );

        var hand = CreateHand(
            participants: [participantSb, participantBb, participantBu],
            smallBlindSeat: 6,
            bigBlindSeat: 2,
            buttonSeat: 4
        );

        hand.Start();

        var events = hand.PullEvents();
        Assert.Equal(13, events.Count);
        Assert.IsType<HandCreatedEvent>(events[0]);
        Assert.IsType<HandStartedEvent>(events[1]);
        Assert.IsType<StageStartedEvent>(events[2]);
        var event4 = Assert.IsType<SmallBlindPostedEvent>(events[3]);
        Assert.Equal(new Nickname("SmallBlind"), event4.Nickname);
        var event5 = Assert.IsType<BigBlindPostedEvent>(events[4]);
        Assert.Equal(new Nickname("BigBlind"), event5.Nickname);
        Assert.IsType<StageFinishedEvent>(events[5]);
        Assert.IsType<StageStartedEvent>(events[6]);
        var event8 = Assert.IsType<HoleCardsDealtEvent>(events[7]);
        Assert.Equal(new Nickname("SmallBlind"), event8.Nickname);
        Assert.Equal(2, event8.Cards.Count);
        var event9 = Assert.IsType<HoleCardsDealtEvent>(events[8]);
        Assert.Equal(new Nickname("BigBlind"), event9.Nickname);
        Assert.Equal(2, event9.Cards.Count);
        var event10 = Assert.IsType<HoleCardsDealtEvent>(events[9]);
        Assert.Equal(new Nickname("Button"), event10.Nickname);
        Assert.Equal(2, event10.Cards.Count);
        Assert.IsType<StageFinishedEvent>(events[10]);
        Assert.IsType<StageStartedEvent>(events[11]);
        var event13 = Assert.IsType<PlayerActionRequestedEvent>(events[12]);
        Assert.Equal(new Nickname("Button"), event13.Nickname);

        hand.SubmitPlayerAction(
            nickname: new Nickname("Button"),
            action: new PlayerAction(PlayerActionType.RaiseBy, new Chips(25))
        );

        events = hand.PullEvents();
        Assert.Equal(2, events.Count);
        var event1a = Assert.IsType<PlayerActedEvent>(events[0]);
        Assert.Equal(new Nickname("Button"), event1a.Nickname);
        var event2a = Assert.IsType<PlayerActionRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("SmallBlind"), event2a.Nickname);

        hand.SubmitPlayerAction(
            nickname: new Nickname("SmallBlind"),
            action: new PlayerAction(PlayerActionType.Fold)
        );

        events = hand.PullEvents();
        Assert.Equal(2, events.Count);
        var event1b = Assert.IsType<PlayerActedEvent>(events[0]);
        Assert.Equal(new Nickname("SmallBlind"), event1b.Nickname);
        var event2b = Assert.IsType<PlayerActionRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("BigBlind"), event2b.Nickname);

        hand.SubmitPlayerAction(
            nickname: new Nickname("BigBlind"),
            action: new PlayerAction(PlayerActionType.CallBy, new Chips(15))
        );

        events = hand.PullEvents();
        Assert.Equal(8, events.Count);
        var event1c = Assert.IsType<PlayerActedEvent>(events[0]);
        Assert.Equal(new Nickname("BigBlind"), event1c.Nickname);
        Assert.IsType<BetsCollectedEvent>(events[1]);
        Assert.IsType<StageFinishedEvent>(events[2]);
        Assert.IsType<StageStartedEvent>(events[3]);
        var event4c = Assert.IsType<BoardCardsDealtEvent>(events[4]);
        Assert.Equal(3, event4c.Cards.Count);
        Assert.IsType<StageFinishedEvent>(events[5]);
        Assert.IsType<StageStartedEvent>(events[6]);
        var event7c = Assert.IsType<PlayerActionRequestedEvent>(events[7]);
        Assert.Equal(new Nickname("BigBlind"), event7c.Nickname);

        hand.SubmitPlayerAction(
            nickname: new Nickname("BigBlind"),
            action: new PlayerAction(PlayerActionType.Check)
        );

        events = hand.PullEvents();
        Assert.Equal(2, events.Count);
        var event1d = Assert.IsType<PlayerActedEvent>(events[0]);
        Assert.Equal(new Nickname("BigBlind"), event1d.Nickname);
        var event2d = Assert.IsType<PlayerActionRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("Button"), event2d.Nickname);

        hand.SubmitPlayerAction(
            nickname: new Nickname("Button"),
            action: new PlayerAction(PlayerActionType.RaiseBy, new Chips(15))
        );

        events = hand.PullEvents();
        Assert.Equal(2, events.Count);
        var event1e = Assert.IsType<PlayerActedEvent>(events[0]);
        Assert.Equal(new Nickname("Button"), event1e.Nickname);
        var event2e = Assert.IsType<PlayerActionRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("BigBlind"), event2e.Nickname);

        hand.SubmitPlayerAction(
            nickname: new Nickname("BigBlind"),
            action: new PlayerAction(PlayerActionType.Fold)
        );

        events = hand.PullEvents();
        Assert.Equal(16, events.Count);
        var event1f = Assert.IsType<PlayerActedEvent>(events[0]);
        Assert.Equal(new Nickname("BigBlind"), event1f.Nickname);
        var event2f = Assert.IsType<BetRefundedEvent>(events[1]);
        Assert.Equal(new Nickname("Button"), event2f.Nickname);
        Assert.Equal(new Chips(15), event2f.Amount);
        Assert.IsType<StageFinishedEvent>(events[2]);
        Assert.IsType<StageStartedEvent>(events[3]);
        Assert.IsType<StageFinishedEvent>(events[4]);
        Assert.IsType<StageStartedEvent>(events[5]);
        Assert.IsType<StageFinishedEvent>(events[6]);
        Assert.IsType<StageStartedEvent>(events[7]);
        Assert.IsType<StageFinishedEvent>(events[8]);
        Assert.IsType<StageStartedEvent>(events[9]);
        Assert.IsType<StageFinishedEvent>(events[10]);
        Assert.IsType<StageStartedEvent>(events[11]);
        var event13f = Assert.IsType<HoleCardsMuckedEvent>(events[12]);
        Assert.Equal(new Nickname("Button"), event13f.Nickname);
        var event14f = Assert.IsType<SidePotAwardedEvent>(events[13]);
        Assert.Equal([new Nickname("Button")], event14f.Winners);
        Assert.Equal(new Chips(55), event14f.SidePot.TotalAmount);
        Assert.IsType<StageFinishedEvent>(events[14]);
        Assert.IsType<HandFinishedEvent>(events[15]);
    }

    [Fact]
    public void TestFlowPreflopFold()
    {
        var participantSb = CreateParticipant(
            nickname: "SmallBlind",
            seat: 6
        );
        var participantBb = CreateParticipant(
            nickname: "BigBlind",
            seat: 2
        );
        var participantBu = CreateParticipant(
            nickname: "Button",
            seat: 4
        );

        var hand = CreateHand(
            participants: [participantSb, participantBb, participantBu],
            smallBlindSeat: 6,
            bigBlindSeat: 2,
            buttonSeat: 4
        );
        hand.Start();

        var events = hand.PullEvents();
        Assert.Equal(13, events.Count);

        hand.SubmitPlayerAction(
            nickname: new Nickname("Button"),
            action: new PlayerAction(PlayerActionType.Fold)
        );

        events = hand.PullEvents();
        Assert.Equal(2, events.Count);

        hand.SubmitPlayerAction(
            nickname: new Nickname("SmallBlind"),
            action: new PlayerAction(PlayerActionType.Fold)
        );

        events = hand.PullEvents();
        Assert.Equal(21, events.Count);

        Assert.IsType<PlayerActedEvent>(events[0]);
        var event2 = Assert.IsType<BetRefundedEvent>(events[1]);
        Assert.Equal(new Chips(5), event2.Amount);
        Assert.IsType<BetsCollectedEvent>(events[2]);
        Assert.IsType<StageFinishedEvent>(events[3]);
        Assert.IsType<StageStartedEvent>(events[4]);
        Assert.IsType<StageFinishedEvent>(events[5]);
        Assert.IsType<StageStartedEvent>(events[6]);
        Assert.IsType<StageFinishedEvent>(events[7]);
        Assert.IsType<StageStartedEvent>(events[8]);
        Assert.IsType<StageFinishedEvent>(events[9]);
        Assert.IsType<StageStartedEvent>(events[10]);
        Assert.IsType<StageFinishedEvent>(events[11]);
        Assert.IsType<StageStartedEvent>(events[12]);
        Assert.IsType<StageFinishedEvent>(events[13]);
        Assert.IsType<StageStartedEvent>(events[14]);
        Assert.IsType<StageFinishedEvent>(events[15]);
        Assert.IsType<StageStartedEvent>(events[16]);
        Assert.IsType<HoleCardsMuckedEvent>(events[17]);
        var event18 = Assert.IsType<SidePotAwardedEvent>(events[18]);
        Assert.Equal([new Nickname("BigBlind")], event18.Winners);
        Assert.Equal(new Chips(10), event18.SidePot.TotalAmount);
        Assert.IsType<StageFinishedEvent>(events[19]);
        Assert.IsType<HandFinishedEvent>(events[20]);
    }

    [Fact]
    public void TestFlowHeadsUp()
    {
        var participantSb = CreateParticipant(
            nickname: "SmallBlind",
            seat: 2
        );
        var participantBb = CreateParticipant(
            nickname: "BigBlind",
            seat: 1
        );

        var hand = CreateHand(
            participants: [participantSb, participantBb],
            smallBlindSeat: 2,
            bigBlindSeat: 1,
            buttonSeat: 2
        );

        hand.Start();

        var events = hand.PullEvents();
        Assert.Equal(12, events.Count);
        Assert.IsType<HandCreatedEvent>(events[0]);
        Assert.IsType<HandStartedEvent>(events[1]);
        Assert.IsType<StageStartedEvent>(events[2]);
        var event4 = Assert.IsType<SmallBlindPostedEvent>(events[3]);
        Assert.Equal(new Nickname("SmallBlind"), event4.Nickname);
        Assert.Equal(new Chips(5), event4.Amount);
        var event5 = Assert.IsType<BigBlindPostedEvent>(events[4]);
        Assert.Equal(new Nickname("BigBlind"), event5.Nickname);
        Assert.Equal(new Chips(10), event5.Amount);
        Assert.IsType<StageFinishedEvent>(events[5]);
        Assert.IsType<StageStartedEvent>(events[6]);
        var event8 = Assert.IsType<HoleCardsDealtEvent>(events[7]);
        Assert.Equal(new Nickname("SmallBlind"), event8.Nickname);
        Assert.Equal(2, event8.Cards.Count);
        var event9 = Assert.IsType<HoleCardsDealtEvent>(events[8]);
        Assert.Equal(new Nickname("BigBlind"), event9.Nickname);
        Assert.Equal(2, event9.Cards.Count);
        Assert.IsType<StageFinishedEvent>(events[9]);
        Assert.IsType<StageStartedEvent>(events[10]);
        var event12 = Assert.IsType<PlayerActionRequestedEvent>(events[11]);
        Assert.Equal(new Nickname("SmallBlind"), event12.Nickname);

        hand.SubmitPlayerAction(
            nickname: new Nickname("SmallBlind"),
            action: new PlayerAction(PlayerActionType.RaiseBy, new Chips(20))
        );

        events = hand.PullEvents();
        Assert.Equal(2, events.Count);
        var event1a = Assert.IsType<PlayerActedEvent>(events[0]);
        Assert.Equal(new Nickname("SmallBlind"), event1a.Nickname);
        var event2a = Assert.IsType<PlayerActionRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("BigBlind"), event2a.Nickname);

        hand.SubmitPlayerAction(
            nickname: new Nickname("BigBlind"),
            action: new PlayerAction(PlayerActionType.CallBy, new Chips(15))
        );

        events = hand.PullEvents();
        Assert.Equal(8, events.Count);
        var event1b = Assert.IsType<PlayerActedEvent>(events[0]);
        Assert.Equal(new Nickname("BigBlind"), event1b.Nickname);
        Assert.IsType<BetsCollectedEvent>(events[1]);
        Assert.IsType<StageFinishedEvent>(events[2]);
        Assert.IsType<StageStartedEvent>(events[3]);
        var event4b = Assert.IsType<BoardCardsDealtEvent>(events[4]);
        Assert.Equal(3, event4b.Cards.Count);
        Assert.IsType<StageFinishedEvent>(events[5]);
        Assert.IsType<StageStartedEvent>(events[6]);
        var event7b = Assert.IsType<PlayerActionRequestedEvent>(events[7]);
        Assert.Equal(new Nickname("BigBlind"), event7b.Nickname);

        hand.SubmitPlayerAction(
            nickname: new Nickname("BigBlind"),
            action: new PlayerAction(PlayerActionType.Check)
        );

        events = hand.PullEvents();
        Assert.Equal(2, events.Count);
        var event1c = Assert.IsType<PlayerActedEvent>(events[0]);
        Assert.Equal(new Nickname("BigBlind"), event1c.Nickname);
        var event2c = Assert.IsType<PlayerActionRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("SmallBlind"), event2c.Nickname);

        hand.SubmitPlayerAction(
            nickname: new Nickname("SmallBlind"),
            action: new PlayerAction(PlayerActionType.RaiseBy, new Chips(15))
        );

        events = hand.PullEvents();
        Assert.Equal(2, events.Count);
        var event1d = Assert.IsType<PlayerActedEvent>(events[0]);
        Assert.Equal(new Nickname("SmallBlind"), event1d.Nickname);
        var event2d = Assert.IsType<PlayerActionRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("BigBlind"), event2d.Nickname);

        hand.SubmitPlayerAction(
            nickname: new Nickname("BigBlind"),
            action: new PlayerAction(PlayerActionType.Fold)
        );

        events = hand.PullEvents();
        Assert.Equal(16, events.Count);
        var event1e = Assert.IsType<PlayerActedEvent>(events[0]);
        Assert.Equal(new Nickname("BigBlind"), event1e.Nickname);
        var event2e = Assert.IsType<BetRefundedEvent>(events[1]);
        Assert.Equal(new Nickname("SmallBlind"), event2e.Nickname);
        Assert.Equal(new Chips(15), event2e.Amount);
        Assert.IsType<StageFinishedEvent>(events[2]);
        Assert.IsType<StageStartedEvent>(events[3]);
        Assert.IsType<StageFinishedEvent>(events[4]);
        Assert.IsType<StageStartedEvent>(events[5]);
        Assert.IsType<StageFinishedEvent>(events[6]);
        Assert.IsType<StageStartedEvent>(events[7]);
        Assert.IsType<StageFinishedEvent>(events[8]);
        Assert.IsType<StageStartedEvent>(events[9]);
        Assert.IsType<StageFinishedEvent>(events[10]);
        Assert.IsType<StageStartedEvent>(events[11]);
        var event13e = Assert.IsType<HoleCardsMuckedEvent>(events[12]);
        Assert.Equal(new Nickname("SmallBlind"), event13e.Nickname);
        var event14e = Assert.IsType<SidePotAwardedEvent>(events[13]);
        Assert.Equal([new Nickname("SmallBlind")], event14e.Winners);
        Assert.Equal(new Chips(50), event14e.SidePot.TotalAmount);
        Assert.IsType<StageFinishedEvent>(events[14]);
        Assert.IsType<HandFinishedEvent>(events[15]);
    }

    private Hand CreateHand(
        List<Participant> participants,
        int maxSeat = 6,
        int smallBlindSeat = 1,
        int bigBlindSeat = 2,
        int buttonSeat = 6,
        int smallBlind = 5,
        int bigBlind = 10
    )
    {
        return Hand.FromScratch(
            uid: new HandUid(Guid.NewGuid()),
            tableUid: new TableUid(Guid.NewGuid()),
            tableType: TableType.Cash,
            rules: new()
            {
                Game = Game.NoLimitHoldem,
                SmallBlind = new Chips(smallBlind),
                BigBlind = new Chips(bigBlind)
            },
            positions: new()
            {
                SmallBlind = new Seat(smallBlindSeat),
                BigBlind = new Seat(bigBlindSeat),
                Button = new Seat(buttonSeat),
                Max = new Seat(maxSeat)
            },
            participants: participants,
            randomizer: _randomizer,
            evaluator: _evaluator
        );
    }

    private Participant CreateParticipant(string nickname, int seat, int stack = 1000)
    {
        return new Participant
        {
            Nickname = new Nickname(nickname),
            Seat = new Seat(seat),
            Stack = new Chips(stack)
        };
    }
}

public class NoLimitHoldem9MaxHandTest
{
    private readonly StubRandomizer _randomizer = new();
    private readonly StubEvaluator _evaluator = new();

    [Fact]
    public void TestFromScratch()
    {
        var handUid = new HandUid(Guid.NewGuid());
        var tableUid = new TableUid(Guid.NewGuid());
        var participantSb = new Participant
        {
            Nickname = new Nickname("SmallBlind"),
            Seat = new Seat(1),
            Stack = new Chips(1000)
        };
        var participantBb = new Participant
        {
            Nickname = new Nickname("BigBlind"),
            Seat = new Seat(2),
            Stack = new Chips(900)
        };
        var participantUtg1 = new Participant
        {
            Nickname = new Nickname("UnderTheGun1"),
            Seat = new Seat(3),
            Stack = new Chips(800)
        };
        var participantUtg2 = new Participant
        {
            Nickname = new Nickname("UnderTheGun2"),
            Seat = new Seat(4),
            Stack = new Chips(700)
        };
        var participantUtg3 = new Participant
        {
            Nickname = new Nickname("UnderTheGun3"),
            Seat = new Seat(5),
            Stack = new Chips(600)
        };
        var participantEp = new Participant
        {
            Nickname = new Nickname("Early"),
            Seat = new Seat(6),
            Stack = new Chips(500)
        };
        var participantMp = new Participant
        {
            Nickname = new Nickname("Middle"),
            Seat = new Seat(7),
            Stack = new Chips(400)
        };
        var participantCo = new Participant
        {
            Nickname = new Nickname("CutOff"),
            Seat = new Seat(8),
            Stack = new Chips(300)
        };
        var participantBu = new Participant
        {
            Nickname = new Nickname("Button"),
            Seat = new Seat(9),
            Stack = new Chips(200)
        };

        var hand = Hand.FromScratch(
            uid: handUid,
            tableUid: tableUid,
            tableType: TableType.Cash,
            rules: new()
            {
                Game = Game.NoLimitHoldem,
                SmallBlind = new Chips(5),
                BigBlind = new Chips(10)
            },
            positions: new()
            {
                SmallBlind = new Seat(1),
                BigBlind = new Seat(2),
                Button = new Seat(9),
                Max = new Seat(9)
            },
            participants: [participantSb, participantBb, participantUtg1, participantUtg2, participantUtg3, participantEp, participantMp, participantCo, participantBu],
            randomizer: _randomizer,
            evaluator: _evaluator
        );

        Assert.Equal(handUid, hand.Uid);
        Assert.Equal(Game.NoLimitHoldem, hand.Rules.Game);
        Assert.Equal(new Chips(5), hand.Rules.SmallBlind);
        Assert.Equal(new Chips(10), hand.Rules.BigBlind);
        Assert.Equal(new Seat(1), hand.Table.Positions.SmallBlind);
        Assert.Equal(new Seat(2), hand.Table.Positions.BigBlind);
        Assert.Equal(new Seat(9), hand.Table.Positions.Button);
        Assert.Equal(new Seat(9), hand.Table.Positions.Max);
        Assert.Equal(new Chips(0), hand.Pot.TotalAmount);
        Assert.IsType<StandardDeck>(hand.Deck);

        var events = hand.PullEvents();
        Assert.Single(events);
        var @event = Assert.IsType<HandCreatedEvent>(events[0]);
        Assert.Equal(Game.NoLimitHoldem, @event.Rules.Game);
        Assert.Equal(new Chips(5), @event.Rules.SmallBlind);
        Assert.Equal(new Chips(10), @event.Rules.BigBlind);
        Assert.Equal(9, @event.Participants.Count);
        Assert.Equal(participantSb, @event.Participants[0]);
        Assert.Equal(participantBb, @event.Participants[1]);
        Assert.Equal(participantUtg1, @event.Participants[2]);
        Assert.Equal(participantUtg2, @event.Participants[3]);
        Assert.Equal(participantUtg3, @event.Participants[4]);
        Assert.Equal(participantEp, @event.Participants[5]);
        Assert.Equal(participantMp, @event.Participants[6]);
        Assert.Equal(participantCo, @event.Participants[7]);
        Assert.Equal(participantBu, @event.Participants[8]);
    }
}

public class PotLimitOmaha6MaxHandTest
{
    private readonly StubRandomizer _randomizer = new();
    private readonly StubEvaluator _evaluator = new();

    [Fact]
    public void TestFromScratch()
    {
        var handUid = new HandUid(Guid.NewGuid());
        var tableUid = new TableUid(Guid.NewGuid());
        var participantSb = new Participant
        {
            Nickname = new Nickname("SmallBlind"),
            Seat = new Seat(1),
            Stack = new Chips(1000)
        };
        var participantBb = new Participant
        {
            Nickname = new Nickname("BigBlind"),
            Seat = new Seat(2),
            Stack = new Chips(900)
        };
        var participantEp = new Participant
        {
            Nickname = new Nickname("Early"),
            Seat = new Seat(3),
            Stack = new Chips(800)
        };
        var participantMp = new Participant
        {
            Nickname = new Nickname("Middle"),
            Seat = new Seat(4),
            Stack = new Chips(700)
        };
        var participantCo = new Participant
        {
            Nickname = new Nickname("CutOff"),
            Seat = new Seat(5),
            Stack = new Chips(600)
        };
        var participantBu = new Participant
        {
            Nickname = new Nickname("Button"),
            Seat = new Seat(6),
            Stack = new Chips(500)
        };

        var hand = Hand.FromScratch(
            uid: handUid,
            tableUid: tableUid,
            tableType: TableType.Cash,
            rules: new()
            {
                Game = Game.PotLimitOmaha,
                SmallBlind = new Chips(5),
                BigBlind = new Chips(10)
            },
            positions: new()
            {
                SmallBlind = new Seat(1),
                BigBlind = new Seat(2),
                Button = new Seat(6),
                Max = new Seat(6)
            },
            participants: [participantSb, participantBb, participantEp, participantMp, participantCo, participantBu],
            randomizer: _randomizer,
            evaluator: _evaluator
        );

        Assert.Equal(handUid, hand.Uid);
        Assert.Equal(Game.PotLimitOmaha, hand.Rules.Game);
        Assert.Equal(new Chips(5), hand.Rules.SmallBlind);
        Assert.Equal(new Chips(10), hand.Rules.BigBlind);
        Assert.Equal(new Seat(1), hand.Table.Positions.SmallBlind);
        Assert.Equal(new Seat(2), hand.Table.Positions.BigBlind);
        Assert.Equal(new Seat(6), hand.Table.Positions.Button);
        Assert.Equal(new Seat(6), hand.Table.Positions.Max);
        Assert.Equal(new Chips(0), hand.Pot.TotalAmount);
        Assert.IsType<StandardDeck>(hand.Deck);

        var events = hand.PullEvents();
        Assert.Single(events);
        var @event = Assert.IsType<HandCreatedEvent>(events[0]);
        Assert.Equal(Game.PotLimitOmaha, @event.Rules.Game);
        Assert.Equal(new Chips(5), @event.Rules.SmallBlind);
        Assert.Equal(new Chips(10), @event.Rules.BigBlind);
        Assert.Equal(6, @event.Participants.Count);
        Assert.Equal(participantSb, @event.Participants[0]);
        Assert.Equal(participantBb, @event.Participants[1]);
        Assert.Equal(participantEp, @event.Participants[2]);
        Assert.Equal(participantMp, @event.Participants[3]);
        Assert.Equal(participantCo, @event.Participants[4]);
        Assert.Equal(participantBu, @event.Participants[5]);
    }
}

public class PotLimitOmaha9MaxHandTest
{
    private readonly StubRandomizer _randomizer = new();
    private readonly StubEvaluator _evaluator = new();

    [Fact]
    public void TestFromScratch()
    {
        var handUid = new HandUid(Guid.NewGuid());
        var tableUid = new TableUid(Guid.NewGuid());
        var participantSb = new Participant
        {
            Nickname = new Nickname("SmallBlind"),
            Seat = new Seat(1),
            Stack = new Chips(1000)
        };
        var participantBb = new Participant
        {
            Nickname = new Nickname("BigBlind"),
            Seat = new Seat(2),
            Stack = new Chips(900)
        };
        var participantUtg1 = new Participant
        {
            Nickname = new Nickname("UnderTheGun1"),
            Seat = new Seat(3),
            Stack = new Chips(800)
        };
        var participantUtg2 = new Participant
        {
            Nickname = new Nickname("UnderTheGun2"),
            Seat = new Seat(4),
            Stack = new Chips(700)
        };
        var participantUtg3 = new Participant
        {
            Nickname = new Nickname("UnderTheGun3"),
            Seat = new Seat(5),
            Stack = new Chips(600)
        };
        var participantEp = new Participant
        {
            Nickname = new Nickname("Early"),
            Seat = new Seat(6),
            Stack = new Chips(500)
        };
        var participantMp = new Participant
        {
            Nickname = new Nickname("Middle"),
            Seat = new Seat(7),
            Stack = new Chips(400)
        };
        var participantCo = new Participant
        {
            Nickname = new Nickname("CutOff"),
            Seat = new Seat(8),
            Stack = new Chips(300)
        };
        var participantBu = new Participant
        {
            Nickname = new Nickname("Button"),
            Seat = new Seat(9),
            Stack = new Chips(200)
        };

        var hand = Hand.FromScratch(
            uid: handUid,
            tableUid: tableUid,
            tableType: TableType.Cash,
            rules: new()
            {
                Game = Game.PotLimitOmaha,
                SmallBlind = new Chips(5),
                BigBlind = new Chips(10)
            },
            positions: new()
            {
                SmallBlind = new Seat(1),
                BigBlind = new Seat(2),
                Button = new Seat(9),
                Max = new Seat(9)
            },
            participants: [participantSb, participantBb, participantUtg1, participantUtg2, participantUtg3, participantEp, participantMp, participantCo, participantBu],
            randomizer: _randomizer,
            evaluator: _evaluator
        );

        Assert.Equal(handUid, hand.Uid);
        Assert.Equal(Game.PotLimitOmaha, hand.Rules.Game);
        Assert.Equal(new Chips(5), hand.Rules.SmallBlind);
        Assert.Equal(new Chips(10), hand.Rules.BigBlind);
        Assert.Equal(new Seat(1), hand.Table.Positions.SmallBlind);
        Assert.Equal(new Seat(2), hand.Table.Positions.BigBlind);
        Assert.Equal(new Seat(9), hand.Table.Positions.Button);
        Assert.Equal(new Seat(9), hand.Table.Positions.Max);
        Assert.Equal(new Chips(0), hand.Pot.TotalAmount);
        Assert.IsType<StandardDeck>(hand.Deck);

        var events = hand.PullEvents();
        Assert.Single(events);
        var @event = Assert.IsType<HandCreatedEvent>(events[0]);
        Assert.Equal(Game.PotLimitOmaha, @event.Rules.Game);
        Assert.Equal(new Chips(5), @event.Rules.SmallBlind);
        Assert.Equal(new Chips(10), @event.Rules.BigBlind);
        Assert.Equal(9, @event.Participants.Count);
        Assert.Equal(participantSb, @event.Participants[0]);
        Assert.Equal(participantBb, @event.Participants[1]);
        Assert.Equal(participantUtg1, @event.Participants[2]);
        Assert.Equal(participantUtg2, @event.Participants[3]);
        Assert.Equal(participantUtg3, @event.Participants[4]);
        Assert.Equal(participantEp, @event.Participants[5]);
        Assert.Equal(participantMp, @event.Participants[6]);
        Assert.Equal(participantCo, @event.Participants[7]);
        Assert.Equal(participantBu, @event.Participants[8]);
    }
}
