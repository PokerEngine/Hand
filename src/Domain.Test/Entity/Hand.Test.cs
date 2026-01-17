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
        var @event = Assert.IsType<HandIsCreatedEvent>(events[0]);
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
            new HandIsCreatedEvent
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
            new HandIsStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageIsStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new SmallBlindIsPostedEvent
            {
                Nickname = participantSb.Nickname,
                Amount = new Chips(5),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new BigBlindIsPostedEvent
            {
                Nickname = participantBb.Nickname,
                Amount = new Chips(10),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageIsFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageIsStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new HoleCardsAreDealtEvent
            {
                Nickname = participantSb.Nickname,
                Cards = new CardSet([Card.TreyOfClubs, Card.NineOfClubs]),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new HoleCardsAreDealtEvent
            {
                Nickname = participantBb.Nickname,
                Cards = new CardSet([Card.QueenOfClubs, Card.TenOfDiamonds]),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new HoleCardsAreDealtEvent
            {
                Nickname = participantBu.Nickname,
                Cards = new CardSet([Card.SevenOfSpades, Card.EightOfSpades]),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageIsFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageIsStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new DecisionIsRequestedEvent
            {
                Nickname = participantBu.Nickname,
                FoldIsAvailable = true,
                CheckIsAvailable = false,
                CallIsAvailable = true,
                CallToAmount = new Chips(10),
                RaiseIsAvailable = true,
                MinRaiseToAmount = new Chips(20),
                MaxRaiseToAmount = new Chips(800),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new DecisionIsCommittedEvent
            {
                Nickname = participantBu.Nickname,
                Decision = new Decision(DecisionType.RaiseTo, new Chips(25)),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new DecisionIsRequestedEvent
            {
                Nickname = participantSb.Nickname,
                FoldIsAvailable = true,
                CheckIsAvailable = false,
                CallIsAvailable = true,
                CallToAmount = new Chips(25),
                RaiseIsAvailable = true,
                MinRaiseToAmount = new Chips(40),
                MaxRaiseToAmount = new Chips(1000),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new DecisionIsCommittedEvent
            {
                Nickname = participantSb.Nickname,
                Decision = new Decision(DecisionType.Fold, new Chips(0)),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new DecisionIsRequestedEvent
            {
                Nickname = participantBb.Nickname,
                FoldIsAvailable = true,
                CheckIsAvailable = false,
                CallIsAvailable = true,
                CallToAmount = new Chips(25),
                RaiseIsAvailable = true,
                MinRaiseToAmount = new Chips(40),
                MaxRaiseToAmount = new Chips(900),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new DecisionIsCommittedEvent
            {
                Nickname = participantBb.Nickname,
                Decision = new Decision(DecisionType.Call),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageIsFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageIsStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new BoardCardsAreDealtEvent
            {
                Cards = new CardSet([Card.AceOfSpades, Card.SevenOfClubs, Card.DeuceOfDiamonds]),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageIsFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageIsStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new DecisionIsRequestedEvent
            {
                Nickname = participantBb.Nickname,
                FoldIsAvailable = false,
                CheckIsAvailable = true,
                CallIsAvailable = false,
                CallToAmount = new Chips(0),
                RaiseIsAvailable = true,
                MinRaiseToAmount = new Chips(10),
                MaxRaiseToAmount = new Chips(875),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new DecisionIsCommittedEvent
            {
                Nickname = participantBb.Nickname,
                Decision = new Decision(DecisionType.Check, new Chips(0)),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new DecisionIsRequestedEvent
            {
                Nickname = participantBu.Nickname,
                FoldIsAvailable = false,
                CheckIsAvailable = true,
                CallIsAvailable = false,
                CallToAmount = new Chips(0),
                RaiseIsAvailable = true,
                MinRaiseToAmount = new Chips(10),
                MaxRaiseToAmount = new Chips(775),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new DecisionIsCommittedEvent
            {
                Nickname = participantBu.Nickname,
                Decision = new Decision(DecisionType.RaiseTo, new Chips(15)),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new DecisionIsRequestedEvent
            {
                Nickname = participantBb.Nickname,
                FoldIsAvailable = true,
                CheckIsAvailable = false,
                CallIsAvailable = true,
                CallToAmount = new Chips(15),
                RaiseIsAvailable = true,
                MinRaiseToAmount = new Chips(30),
                MaxRaiseToAmount = new Chips(875),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new DecisionIsCommittedEvent
            {
                Nickname = participantBb.Nickname,
                Decision = new Decision(DecisionType.Fold, new Chips(0)),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new RefundIsCommittedEvent
            {
                Nickname = participantBu.Nickname,
                Amount = new Chips(15),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageIsFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageIsStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageIsFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageIsStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageIsFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageIsStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageIsFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageIsStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageIsFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageIsStartedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new HoleCardsAreMuckedEvent
            {
                Nickname = participantBu.Nickname,
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new AwardIsCommittedEvent
            {
                Nicknames = [participantBu.Nickname],
                Amount = new Chips(55),
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new StageIsFinishedEvent
            {
                OccurredAt = new DateTime(2025, 1, 1)
            },
            new HandIsFinishedEvent
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
        Assert.Equal(new Chips(55), hand.Pot.TotalAmount);
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
        hand.CommitDecision(
            nickname: new Nickname("Button"),
            decision: new Decision(DecisionType.RaiseTo, new Chips(25))
        );
        hand.CommitDecision(
            nickname: new Nickname("SmallBlind"),
            decision: new Decision(DecisionType.Fold, new Chips(0))
        );
        hand.CommitDecision(
            nickname: new Nickname("BigBlind"),
            decision: new Decision(DecisionType.Call, new Chips(0))
        );
        hand.CommitDecision(
            nickname: new Nickname("BigBlind"),
            decision: new Decision(DecisionType.Check, new Chips(0))
        );
        hand.CommitDecision(
            nickname: new Nickname("Button"),
            decision: new Decision(DecisionType.RaiseTo, new Chips(15))
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
        Assert.Equal(3, state.Pot.CommittedBets.Count);
        Assert.Equal(new Nickname("SmallBlind"), state.Pot.CommittedBets[0].Nickname);
        Assert.Equal(new Chips(5), state.Pot.CommittedBets[0].Amount);
        Assert.Equal(new Nickname("BigBlind"), state.Pot.CommittedBets[1].Nickname);
        Assert.Equal(new Chips(25), state.Pot.CommittedBets[1].Amount);
        Assert.Equal(new Nickname("Button"), state.Pot.CommittedBets[2].Nickname);
        Assert.Equal(new Chips(25), state.Pot.CommittedBets[2].Amount);
        Assert.Equal(2, state.Pot.UncommittedBets.Count);
        Assert.Equal(new Nickname("BigBlind"), state.Pot.UncommittedBets[0].Nickname);
        Assert.Equal(new Chips(0), state.Pot.UncommittedBets[0].Amount);
        Assert.Equal(new Nickname("Button"), state.Pot.UncommittedBets[1].Nickname);
        Assert.Equal(new Chips(15), state.Pot.UncommittedBets[1].Amount);
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
        Assert.IsType<HandIsCreatedEvent>(events[0]);
        Assert.IsType<HandIsStartedEvent>(events[1]);
        Assert.IsType<StageIsStartedEvent>(events[2]);
        var event4 = Assert.IsType<SmallBlindIsPostedEvent>(events[3]);
        Assert.Equal(new Nickname("SmallBlind"), event4.Nickname);
        var event5 = Assert.IsType<BigBlindIsPostedEvent>(events[4]);
        Assert.Equal(new Nickname("BigBlind"), event5.Nickname);
        Assert.IsType<StageIsFinishedEvent>(events[5]);
        Assert.IsType<StageIsStartedEvent>(events[6]);
        var event8 = Assert.IsType<HoleCardsAreDealtEvent>(events[7]);
        Assert.Equal(new Nickname("SmallBlind"), event8.Nickname);
        var event9 = Assert.IsType<HoleCardsAreDealtEvent>(events[8]);
        Assert.Equal(new Nickname("BigBlind"), event9.Nickname);
        var event10 = Assert.IsType<HoleCardsAreDealtEvent>(events[9]);
        Assert.Equal(new Nickname("Button"), event10.Nickname);
        Assert.IsType<StageIsFinishedEvent>(events[10]);
        Assert.IsType<StageIsStartedEvent>(events[11]);
        var event13 = Assert.IsType<DecisionIsRequestedEvent>(events[12]);
        Assert.Equal(new Nickname("Button"), event13.Nickname);

        hand.CommitDecision(
            nickname: new Nickname("Button"),
            decision: new Decision(DecisionType.RaiseTo, new Chips(25))
        );

        events = hand.PullEvents();
        Assert.Equal(2, events.Count);
        var event1a = Assert.IsType<DecisionIsCommittedEvent>(events[0]);
        Assert.Equal(new Nickname("Button"), event1a.Nickname);
        var event2a = Assert.IsType<DecisionIsRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("SmallBlind"), event2a.Nickname);

        hand.CommitDecision(
            nickname: new Nickname("SmallBlind"),
            decision: new Decision(DecisionType.Fold, new Chips(0))
        );

        events = hand.PullEvents();
        Assert.Equal(2, events.Count);
        var event1b = Assert.IsType<DecisionIsCommittedEvent>(events[0]);
        Assert.Equal(new Nickname("SmallBlind"), event1b.Nickname);
        var event2b = Assert.IsType<DecisionIsRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("BigBlind"), event2b.Nickname);

        hand.CommitDecision(
            nickname: new Nickname("BigBlind"),
            decision: new Decision(DecisionType.Call, new Chips(0))
        );

        events = hand.PullEvents();
        Assert.Equal(7, events.Count);
        var event1c = Assert.IsType<DecisionIsCommittedEvent>(events[0]);
        Assert.Equal(new Nickname("BigBlind"), event1c.Nickname);
        Assert.IsType<StageIsFinishedEvent>(events[1]);
        Assert.IsType<StageIsStartedEvent>(events[2]);
        Assert.IsType<BoardCardsAreDealtEvent>(events[3]);
        Assert.IsType<StageIsFinishedEvent>(events[4]);
        Assert.IsType<StageIsStartedEvent>(events[5]);
        var event7c = Assert.IsType<DecisionIsRequestedEvent>(events[6]);
        Assert.Equal(new Nickname("BigBlind"), event7c.Nickname);

        hand.CommitDecision(
            nickname: new Nickname("BigBlind"),
            decision: new Decision(DecisionType.Check, new Chips(0))
        );

        events = hand.PullEvents();
        Assert.Equal(2, events.Count);
        var event1d = Assert.IsType<DecisionIsCommittedEvent>(events[0]);
        Assert.Equal(new Nickname("BigBlind"), event1d.Nickname);
        var event2d = Assert.IsType<DecisionIsRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("Button"), event2d.Nickname);

        hand.CommitDecision(
            nickname: new Nickname("Button"),
            decision: new Decision(DecisionType.RaiseTo, new Chips(15))
        );

        events = hand.PullEvents();
        Assert.Equal(2, events.Count);
        var event1e = Assert.IsType<DecisionIsCommittedEvent>(events[0]);
        Assert.Equal(new Nickname("Button"), event1e.Nickname);
        var event2e = Assert.IsType<DecisionIsRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("BigBlind"), event2e.Nickname);

        hand.CommitDecision(
            nickname: new Nickname("BigBlind"),
            decision: new Decision(DecisionType.Fold, new Chips(0))
        );

        events = hand.PullEvents();
        Assert.Equal(16, events.Count);
        var event1f = Assert.IsType<DecisionIsCommittedEvent>(events[0]);
        Assert.Equal(new Nickname("BigBlind"), event1f.Nickname);
        var event2f = Assert.IsType<RefundIsCommittedEvent>(events[1]);
        Assert.Equal(new Nickname("Button"), event2f.Nickname);
        Assert.Equal(new Chips(15), event2f.Amount);
        Assert.IsType<StageIsFinishedEvent>(events[2]);
        Assert.IsType<StageIsStartedEvent>(events[3]);
        Assert.IsType<StageIsFinishedEvent>(events[4]);
        Assert.IsType<StageIsStartedEvent>(events[5]);
        Assert.IsType<StageIsFinishedEvent>(events[6]);
        Assert.IsType<StageIsStartedEvent>(events[7]);
        Assert.IsType<StageIsFinishedEvent>(events[8]);
        Assert.IsType<StageIsStartedEvent>(events[9]);
        Assert.IsType<StageIsFinishedEvent>(events[10]);
        Assert.IsType<StageIsStartedEvent>(events[11]);
        var event13f = Assert.IsType<HoleCardsAreMuckedEvent>(events[12]);
        Assert.Equal(new Nickname("Button"), event13f.Nickname);
        var event14f = Assert.IsType<AwardIsCommittedEvent>(events[13]);
        Assert.Equal([new Nickname("Button")], event14f.Nicknames);
        Assert.Equal(new Chips(55), event14f.Amount);
        Assert.IsType<StageIsFinishedEvent>(events[14]);
        Assert.IsType<HandIsFinishedEvent>(events[15]);
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
        Assert.IsType<HandIsCreatedEvent>(events[0]);
        Assert.IsType<HandIsStartedEvent>(events[1]);
        Assert.IsType<StageIsStartedEvent>(events[2]);
        var event4 = Assert.IsType<SmallBlindIsPostedEvent>(events[3]);
        Assert.Equal(new Nickname("SmallBlind"), event4.Nickname);
        Assert.Equal(new Chips(5), event4.Amount);
        var event5 = Assert.IsType<BigBlindIsPostedEvent>(events[4]);
        Assert.Equal(new Nickname("BigBlind"), event5.Nickname);
        Assert.Equal(new Chips(10), event5.Amount);
        Assert.IsType<StageIsFinishedEvent>(events[5]);
        Assert.IsType<StageIsStartedEvent>(events[6]);
        var event8 = Assert.IsType<HoleCardsAreDealtEvent>(events[7]);
        Assert.Equal(new Nickname("SmallBlind"), event8.Nickname);
        Assert.Equal(2, event8.Cards.Count);
        var event9 = Assert.IsType<HoleCardsAreDealtEvent>(events[8]);
        Assert.Equal(new Nickname("BigBlind"), event9.Nickname);
        Assert.Equal(2, event9.Cards.Count);
        Assert.IsType<StageIsFinishedEvent>(events[9]);
        Assert.IsType<StageIsStartedEvent>(events[10]);
        var event12 = Assert.IsType<DecisionIsRequestedEvent>(events[11]);
        Assert.Equal(new Nickname("SmallBlind"), event12.Nickname);

        hand.CommitDecision(
            nickname: new Nickname("SmallBlind"),
            decision: new Decision(DecisionType.RaiseTo, new Chips(25))
        );

        events = hand.PullEvents();
        Assert.Equal(2, events.Count);
        var event1a = Assert.IsType<DecisionIsCommittedEvent>(events[0]);
        Assert.Equal(new Nickname("SmallBlind"), event1a.Nickname);
        var event2a = Assert.IsType<DecisionIsRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("BigBlind"), event2a.Nickname);

        hand.CommitDecision(
            nickname: new Nickname("BigBlind"),
            decision: new Decision(DecisionType.Call, new Chips(0))
        );

        events = hand.PullEvents();
        Assert.Equal(7, events.Count);
        var event1b = Assert.IsType<DecisionIsCommittedEvent>(events[0]);
        Assert.Equal(new Nickname("BigBlind"), event1b.Nickname);
        Assert.IsType<StageIsFinishedEvent>(events[1]);
        Assert.IsType<StageIsStartedEvent>(events[2]);
        Assert.IsType<BoardCardsAreDealtEvent>(events[3]);
        Assert.IsType<StageIsFinishedEvent>(events[4]);
        Assert.IsType<StageIsStartedEvent>(events[5]);
        var event7b = Assert.IsType<DecisionIsRequestedEvent>(events[6]);
        Assert.Equal(new Nickname("BigBlind"), event7b.Nickname);

        hand.CommitDecision(
            nickname: new Nickname("BigBlind"),
            decision: new Decision(DecisionType.Check, new Chips(0))
        );

        events = hand.PullEvents();
        Assert.Equal(2, events.Count);
        var event1c = Assert.IsType<DecisionIsCommittedEvent>(events[0]);
        Assert.Equal(new Nickname("BigBlind"), event1c.Nickname);
        var event2c = Assert.IsType<DecisionIsRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("SmallBlind"), event2c.Nickname);

        hand.CommitDecision(
            nickname: new Nickname("SmallBlind"),
            decision: new Decision(DecisionType.RaiseTo, new Chips(15))
        );

        events = hand.PullEvents();
        Assert.Equal(2, events.Count);
        var event1d = Assert.IsType<DecisionIsCommittedEvent>(events[0]);
        Assert.Equal(new Nickname("SmallBlind"), event1d.Nickname);
        var event2d = Assert.IsType<DecisionIsRequestedEvent>(events[1]);
        Assert.Equal(new Nickname("BigBlind"), event2d.Nickname);

        hand.CommitDecision(
            nickname: new Nickname("BigBlind"),
            decision: new Decision(DecisionType.Fold, new Chips(0))
        );

        events = hand.PullEvents();
        Assert.Equal(16, events.Count);
        var event1e = Assert.IsType<DecisionIsCommittedEvent>(events[0]);
        Assert.Equal(new Nickname("BigBlind"), event1e.Nickname);
        var event2e = Assert.IsType<RefundIsCommittedEvent>(events[1]);
        Assert.Equal(new Nickname("SmallBlind"), event2e.Nickname);
        Assert.Equal(new Chips(15), event2e.Amount);
        Assert.IsType<StageIsFinishedEvent>(events[2]);
        Assert.IsType<StageIsStartedEvent>(events[3]);
        Assert.IsType<StageIsFinishedEvent>(events[4]);
        Assert.IsType<StageIsStartedEvent>(events[5]);
        Assert.IsType<StageIsFinishedEvent>(events[6]);
        Assert.IsType<StageIsStartedEvent>(events[7]);
        Assert.IsType<StageIsFinishedEvent>(events[8]);
        Assert.IsType<StageIsStartedEvent>(events[9]);
        Assert.IsType<StageIsFinishedEvent>(events[10]);
        Assert.IsType<StageIsStartedEvent>(events[11]);
        var event13e = Assert.IsType<HoleCardsAreMuckedEvent>(events[12]);
        Assert.Equal(new Nickname("SmallBlind"), event13e.Nickname);
        var event14e = Assert.IsType<AwardIsCommittedEvent>(events[13]);
        Assert.Equal([new Nickname("SmallBlind")], event14e.Nicknames);
        Assert.Equal(new Chips(50), event14e.Amount);
        Assert.IsType<StageIsFinishedEvent>(events[14]);
        Assert.IsType<HandIsFinishedEvent>(events[15]);
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
        var @event = Assert.IsType<HandIsCreatedEvent>(events[0]);
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
        var @event = Assert.IsType<HandIsCreatedEvent>(events[0]);
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
        var @event = Assert.IsType<HandIsCreatedEvent>(events[0]);
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
