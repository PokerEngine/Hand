using Domain.Entity;
using Domain.Event;
using Domain.Test.Service.Evaluator;
using Domain.Test.Service.Randomizer;
using Domain.ValueObject;
using System.Collections.Immutable;

namespace Domain.Test.Entity;

public class HoldemNoLimit6MaxHandTest
{
    private readonly FakeRandomizer _randomizer = new();
    private readonly FakeEvaluator _evaluator = new();

    [Fact]
    public void TestFromScratch()
    {
        var handUid = new HandUid(Guid.NewGuid());
        var participantSb = new Participant(
            nickname: new Nickname("SmallBlind"),
            seat: new Seat(1),
            stake: new Chips(1000)
        );
        var participantBb = new Participant(
            nickname: new Nickname("BigBlind"),
            seat: new Seat(2),
            stake: new Chips(900)
        );
        var participantEp = new Participant(
            nickname: new Nickname("Early"),
            seat: new Seat(3),
            stake: new Chips(800)
        );
        var participantMp = new Participant(
            nickname: new Nickname("Middle"),
            seat: new Seat(4),
            stake: new Chips(700)
        );
        var participantCo = new Participant(
            nickname: new Nickname("CutOff"),
            seat: new Seat(5),
            stake: new Chips(600)
        );
        var participantBu = new Participant(
            nickname: new Nickname("Button"),
            seat: new Seat(6),
            stake: new Chips(500)
        );
        var eventBus = new EventBus();

        var events = new List<HandIsCreatedEvent>();
        var listener = (HandIsCreatedEvent @event) => events.Add(@event);
        eventBus.Subscribe(listener);

        var hand = Hand.FromScratch(
            uid: handUid,
            game: Game.HoldemNoLimit,
            smallBlind: new Chips(5),
            bigBlind: new Chips(10),
            maxSeat: new Seat(6),
            smallBlindSeat: new Seat(1),
            bigBlindSeat: new Seat(2),
            buttonSeat: new Seat(6),
            participants: [participantSb, participantBb, participantEp, participantMp, participantCo, participantBu],
            randomizer: _randomizer,
            evaluator: _evaluator,
            eventBus: eventBus
        );

        Assert.Equal(handUid, hand.Uid);
        Assert.Equal(Game.HoldemNoLimit, hand.Game);
        Assert.Equal(new Seat(6), hand.Table.MaxSeat);
        Assert.IsType<NoLimitPot>(hand.Pot);
        Assert.IsType<StandardDeck>(hand.Deck);

        Assert.Single(events);
        Assert.Equal(Game.HoldemNoLimit, events[0].Game);
        Assert.Equal(new Chips(5), events[0].SmallBlind);
        Assert.Equal(new Chips(10), events[0].BigBlind);
        Assert.Equal(6, events[0].Participants.Count);
        Assert.Equal(participantSb, events[0].Participants[0]);
        Assert.Equal(participantBb, events[0].Participants[1]);
        Assert.Equal(participantEp, events[0].Participants[2]);
        Assert.Equal(participantMp, events[0].Participants[3]);
        Assert.Equal(participantCo, events[0].Participants[4]);
        Assert.Equal(participantBu, events[0].Participants[5]);
    }

    [Fact]
    public void TestFromEvents()
    {
        var handUid = new HandUid(Guid.NewGuid());
        var participantSb = new Participant(
            nickname: new Nickname("SmallBlind"),
            seat: new Seat(1),
            stake: new Chips(1000)
        );
        var participantBb = new Participant(
            nickname: new Nickname("BigBlind"),
            seat: new Seat(2),
            stake: new Chips(900)
        );
        var participantBu = new Participant(
            nickname: new Nickname("Button"),
            seat: new Seat(6),
            stake: new Chips(800)
        );

        var events = new List<BaseEvent>
        {
            new HandIsCreatedEvent(
                Game: Game.HoldemNoLimit,
                SmallBlind: new Chips(5),
                BigBlind: new Chips(10),
                MaxSeat: new Seat(6),
                SmallBlindSeat: new Seat(1),
                BigBlindSeat: new Seat(2),
                ButtonSeat: new Seat(6),
                Participants: [participantSb, participantBb, participantBu],
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new PlayerConnectedEvent(
                Nickname: participantSb.Nickname,
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new PlayerConnectedEvent(
                Nickname: participantBb.Nickname,
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new PlayerConnectedEvent(
                Nickname: participantBu.Nickname,
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new HandIsStartedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new StageIsStartedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new SmallBlindIsPostedEvent(
                Nickname: participantSb.Nickname,
                Amount: new Chips(5),
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new BigBlindIsPostedEvent(
                Nickname: participantBb.Nickname,
                Amount: new Chips(10),
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new StageIsFinishedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new StageIsStartedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new HoleCardsAreDealtEvent(
                Nickname: participantSb.Nickname,
                Cards: new CardSet([Card.TreyOfClubs, Card.NineOfClubs]),
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new HoleCardsAreDealtEvent(
                Nickname: participantBb.Nickname,
                Cards: new CardSet([Card.QueenOfClubs, Card.TenOfDiamonds]),
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new HoleCardsAreDealtEvent(
                Nickname: participantBu.Nickname,
                Cards: new CardSet([Card.SevenOfSpades, Card.EightOfSpades]),
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new StageIsFinishedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new StageIsStartedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new DecisionIsRequestedEvent(
                Nickname: participantBu.Nickname,
                FoldIsAvailable: true,
                CheckIsAvailable: false,
                CallIsAvailable: true,
                CallToAmount: new Chips(10),
                RaiseIsAvailable: true,
                MinRaiseToAmount: new Chips(20),
                MaxRaiseToAmount: new Chips(800),
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new DecisionIsCommittedEvent(
                Nickname: participantBu.Nickname,
                Decision: new Decision(DecisionType.RaiseTo, new Chips(25)),
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new DecisionIsRequestedEvent(
                Nickname: participantSb.Nickname,
                FoldIsAvailable: true,
                CheckIsAvailable: false,
                CallIsAvailable: true,
                CallToAmount: new Chips(25),
                RaiseIsAvailable: true,
                MinRaiseToAmount: new Chips(40),
                MaxRaiseToAmount: new Chips(1000),
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new DecisionIsCommittedEvent(
                Nickname: participantSb.Nickname,
                Decision: new Decision(DecisionType.Fold, new Chips(0)),
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new DecisionIsRequestedEvent(
                Nickname: participantBb.Nickname,
                FoldIsAvailable: true,
                CheckIsAvailable: false,
                CallIsAvailable: true,
                CallToAmount: new Chips(25),
                RaiseIsAvailable: true,
                MinRaiseToAmount: new Chips(40),
                MaxRaiseToAmount: new Chips(900),
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new DecisionIsCommittedEvent(
                Nickname: participantBb.Nickname,
                Decision: new Decision(DecisionType.CallTo, new Chips(25)),
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new StageIsFinishedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new StageIsStartedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new BoardCardsAreDealtEvent(
                Cards: new CardSet([Card.AceOfSpades, Card.SevenOfClubs, Card.DeuceOfDiamonds]),
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new StageIsFinishedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new StageIsStartedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new DecisionIsRequestedEvent(
                Nickname: participantBb.Nickname,
                FoldIsAvailable: false,
                CheckIsAvailable: true,
                CallIsAvailable: false,
                CallToAmount: new Chips(0),
                RaiseIsAvailable: true,
                MinRaiseToAmount: new Chips(10),
                MaxRaiseToAmount: new Chips(875),
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new DecisionIsCommittedEvent(
                Nickname: participantBb.Nickname,
                Decision: new Decision(DecisionType.Check, new Chips(0)),
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new DecisionIsRequestedEvent(
                Nickname: participantBu.Nickname,
                FoldIsAvailable: false,
                CheckIsAvailable: true,
                CallIsAvailable: false,
                CallToAmount: new Chips(0),
                RaiseIsAvailable: true,
                MinRaiseToAmount: new Chips(10),
                MaxRaiseToAmount: new Chips(775),
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new DecisionIsCommittedEvent(
                Nickname: participantBu.Nickname,
                Decision: new Decision(DecisionType.RaiseTo, new Chips(15)),
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new DecisionIsRequestedEvent(
                Nickname: participantBb.Nickname,
                FoldIsAvailable: true,
                CheckIsAvailable: false,
                CallIsAvailable: true,
                CallToAmount: new Chips(15),
                RaiseIsAvailable: true,
                MinRaiseToAmount: new Chips(30),
                MaxRaiseToAmount: new Chips(875),
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new DecisionIsCommittedEvent(
                Nickname: participantBb.Nickname,
                Decision: new Decision(DecisionType.Fold, new Chips(0)),
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new StageIsFinishedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new StageIsStartedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new StageIsFinishedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new StageIsStartedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new StageIsFinishedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new StageIsStartedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new StageIsFinishedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new StageIsStartedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new StageIsFinishedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new StageIsStartedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new RefundIsCommittedEvent(
                Nickname: participantBu.Nickname,
                Amount: new Chips(15),
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new HoleCardsAreMuckedEvent(
                Nickname: participantBu.Nickname,
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new WinWithoutShowdownIsCommittedEvent(
                Nickname: participantBu.Nickname,
                Amount: new Chips(55),
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new StageIsFinishedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
            new HandIsFinishedEvent(
                OccuredAt: new DateTime(2025, 1, 1)
            ),
        };

        var hand = Hand.FromEvents(
            uid: handUid,
            randomizer: _randomizer,
            evaluator: _evaluator,
            events: events
        );

        Assert.Equal(handUid, hand.Uid);
        Assert.Equal(Game.HoldemNoLimit, hand.Game);
        Assert.Equal(new Seat(6), hand.Table.MaxSeat);
        Assert.IsType<NoLimitPot>(hand.Pot);
        Assert.IsType<StandardDeck>(hand.Deck);

        Assert.Equal(new CardSet([Card.AceOfSpades, Card.SevenOfClubs, Card.DeuceOfDiamonds]), hand.Table.BoardCards);

        var playerSb = hand.Table.GetPlayerByNickname(participantSb.Nickname);
        var playerBb = hand.Table.GetPlayerByNickname(participantBb.Nickname);
        var playerBu = hand.Table.GetPlayerByNickname(participantBu.Nickname);

        Assert.Equal(new CardSet([Card.TreyOfClubs, Card.NineOfClubs]), playerSb.HoleCards);
        Assert.Equal(new Chips(995), playerSb.Stake);
        Assert.True(playerSb.IsFolded);

        Assert.Equal(new CardSet([Card.QueenOfClubs, Card.TenOfDiamonds]), playerBb.HoleCards);
        Assert.Equal(new Chips(875), playerBb.Stake);
        Assert.True(playerBb.IsFolded);

        Assert.Equal(new CardSet([Card.SevenOfSpades, Card.EightOfSpades]), playerBu.HoleCards);
        Assert.Equal(new Chips(830), playerBu.Stake);
        Assert.False(playerBu.IsFolded);
    }

    [Fact]
    public void TestConnect()
    {
        var participantSb = CreateParticipant(
            nickname: "SmallBlind",
            seat: 1
        );
        var participantBb = CreateParticipant(
            nickname: "BigBlind",
            seat: 2
        );
        var participantBu = CreateParticipant(
            nickname: "Button",
            seat: 6
        );
        var eventBus = new EventBus();

        var hand = CreateHand(
            participants: [participantSb, participantBb, participantBu]
        );

        var events = new List<PlayerConnectedEvent>();
        var listener = (PlayerConnectedEvent @event) => events.Add(@event);
        eventBus.Subscribe(listener);

        hand.ConnectPlayer(
            nickname: new Nickname("SmallBlind"),
            eventBus: eventBus
        );

        Assert.Single(events);
        Assert.Equal(new Nickname("SmallBlind"), events[0].Nickname);
    }

    [Fact]
    public void TestDisconnect()
    {
        var participantSb = CreateParticipant(
            nickname: "SmallBlind",
            seat: 1
        );
        var participantBb = CreateParticipant(
            nickname: "BigBlind",
            seat: 2
        );
        var participantBu = CreateParticipant(
            nickname: "Button",
            seat: 6
        );
        var eventBus = new EventBus();

        var hand = CreateHand(
            participants: [participantSb, participantBb, participantBu]
        );

        var events = new List<PlayerDisconnectedEvent>();
        var listener = (PlayerDisconnectedEvent @event) => events.Add(@event);
        eventBus.Subscribe(listener);

        hand.ConnectPlayer(
            nickname: new Nickname("SmallBlind"),
            eventBus: eventBus
        );

        hand.DisconnectPlayer(
            nickname: new Nickname("SmallBlind"),
            eventBus: eventBus
        );

        Assert.Single(events);
        Assert.Equal(new Nickname("SmallBlind"), events[0].Nickname);
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
        var eventBus = new EventBus();

        var hand = CreateHand(
            participants: [participantSb, participantBb, participantBu],
            smallBlindSeat: 6,
            bigBlindSeat: 2,
            buttonSeat: 4
        );

        var events = new List<BaseEvent>();
        var decisionRequestEvents = new List<DecisionIsRequestedEvent>();
        var listener = (BaseEvent @event) => events.Add(@event);
        var decisionRequestListener = (DecisionIsRequestedEvent @event) => decisionRequestEvents.Add(@event);
        eventBus.Subscribe(listener);
        eventBus.Subscribe(decisionRequestListener);

        hand.ConnectPlayer(
            nickname: new Nickname("SmallBlind"),
            eventBus: eventBus
        );
        hand.ConnectPlayer(
            nickname: new Nickname("BigBlind"),
            eventBus: eventBus
        );
        hand.ConnectPlayer(
            nickname: new Nickname("Button"),
            eventBus: eventBus
        );

        hand.Start(eventBus);

        Assert.Equal(15, events.Count);
        Assert.IsType<SmallBlindIsPostedEvent>(events[5]);
        Assert.Equal(new Nickname("SmallBlind"), ((SmallBlindIsPostedEvent)events[5]).Nickname);
        Assert.IsType<BigBlindIsPostedEvent>(events[6]);
        Assert.Equal(new Nickname("BigBlind"), ((BigBlindIsPostedEvent)events[6]).Nickname);
        Assert.IsType<HoleCardsAreDealtEvent>(events[9]);
        Assert.Equal(new Nickname("SmallBlind"), ((HoleCardsAreDealtEvent)events[9]).Nickname);
        Assert.IsType<HoleCardsAreDealtEvent>(events[10]);
        Assert.Equal(new Nickname("BigBlind"), ((HoleCardsAreDealtEvent)events[10]).Nickname);
        Assert.IsType<HoleCardsAreDealtEvent>(events[11]);
        Assert.Equal(new Nickname("Button"), ((HoleCardsAreDealtEvent)events[11]).Nickname);
        Assert.Single(decisionRequestEvents);
        Assert.Equal(new Nickname("Button"), decisionRequestEvents[0].Nickname);

        hand.CommitDecision(
            nickname: new Nickname("Button"),
            decision: new Decision(DecisionType.RaiseTo, new Chips(25)),
            eventBus: eventBus
        );

        Assert.Equal(17, events.Count);
        Assert.Equal(2, decisionRequestEvents.Count);
        Assert.Equal(new Nickname("SmallBlind"), decisionRequestEvents[1].Nickname);

        hand.CommitDecision(
            nickname: new Nickname("SmallBlind"),
            decision: new Decision(DecisionType.Fold, new Chips(0)),
            eventBus: eventBus
        );

        Assert.Equal(19, events.Count);
        Assert.Equal(3, decisionRequestEvents.Count);
        Assert.Equal(new Nickname("BigBlind"), decisionRequestEvents[2].Nickname);

        hand.CommitDecision(
            nickname: new Nickname("BigBlind"),
            decision: new Decision(DecisionType.CallTo, new Chips(25)),
            eventBus: eventBus
        );

        Assert.Equal(26, events.Count);
        Assert.IsType<BoardCardsAreDealtEvent>(events[22]);
        Assert.Equal(4, decisionRequestEvents.Count);
        Assert.Equal(new Nickname("BigBlind"), decisionRequestEvents[3].Nickname);

        hand.CommitDecision(
            nickname: new Nickname("BigBlind"),
            decision: new Decision(DecisionType.Check, new Chips(0)),
            eventBus: eventBus
        );

        Assert.Equal(28, events.Count);
        Assert.Equal(5, decisionRequestEvents.Count);
        Assert.Equal(new Nickname("Button"), decisionRequestEvents[4].Nickname);

        hand.CommitDecision(
            nickname: new Nickname("Button"),
            decision: new Decision(DecisionType.RaiseTo, new Chips(15)),
            eventBus: eventBus
        );

        Assert.Equal(30, events.Count);
        Assert.Equal(6, decisionRequestEvents.Count);
        Assert.Equal(new Nickname("BigBlind"), decisionRequestEvents[5].Nickname);

        hand.CommitDecision(
            nickname: new Nickname("BigBlind"),
            decision: new Decision(DecisionType.Fold, new Chips(0)),
            eventBus: eventBus
        );

        Assert.Equal(46, events.Count);
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
        var eventBus = new EventBus();

        var hand = CreateHand(
            participants: [participantSb, participantBb],
            smallBlindSeat: 2,
            bigBlindSeat: 1,
            buttonSeat: 2
        );

        var events = new List<BaseEvent>();
        var decisionRequestEvents = new List<DecisionIsRequestedEvent>();
        var listener = (BaseEvent @event) => events.Add(@event);
        var decisionRequestListener = (DecisionIsRequestedEvent @event) => decisionRequestEvents.Add(@event);
        eventBus.Subscribe(listener);
        eventBus.Subscribe(decisionRequestListener);

        hand.ConnectPlayer(
            nickname: new Nickname("SmallBlind"),
            eventBus: eventBus
        );
        hand.ConnectPlayer(
            nickname: new Nickname("BigBlind"),
            eventBus: eventBus
        );

        hand.Start(eventBus);

        Assert.IsType<HoleCardsAreDealtEvent>(events[8]);
        Assert.Equal(new Nickname("SmallBlind"), ((HoleCardsAreDealtEvent)events[8]).Nickname);
        Assert.IsType<HoleCardsAreDealtEvent>(events[9]);
        Assert.Equal(new Nickname("BigBlind"), ((HoleCardsAreDealtEvent)events[9]).Nickname);
        Assert.Equal(13, events.Count);
        Assert.Single(decisionRequestEvents);
        Assert.Equal(new Nickname("SmallBlind"), decisionRequestEvents[0].Nickname);

        hand.CommitDecision(
            nickname: new Nickname("SmallBlind"),
            decision: new Decision(DecisionType.RaiseTo, new Chips(25)),
            eventBus: eventBus
        );

        Assert.Equal(15, events.Count);
        Assert.Equal(2, decisionRequestEvents.Count);
        Assert.Equal(new Nickname("BigBlind"), decisionRequestEvents[1].Nickname);

        hand.CommitDecision(
            nickname: new Nickname("BigBlind"),
            decision: new Decision(DecisionType.CallTo, new Chips(25)),
            eventBus: eventBus
        );

        Assert.Equal(22, events.Count);
        Assert.IsType<BoardCardsAreDealtEvent>(events[18]);
        Assert.Equal(3, decisionRequestEvents.Count);
        Assert.Equal(new Nickname("BigBlind"), decisionRequestEvents[2].Nickname);

        hand.CommitDecision(
            nickname: new Nickname("BigBlind"),
            decision: new Decision(DecisionType.Check, new Chips(0)),
            eventBus: eventBus
        );

        Assert.Equal(24, events.Count);
        Assert.Equal(4, decisionRequestEvents.Count);
        Assert.Equal(new Nickname("SmallBlind"), decisionRequestEvents[3].Nickname);

        hand.CommitDecision(
            nickname: new Nickname("SmallBlind"),
            decision: new Decision(DecisionType.RaiseTo, new Chips(15)),
            eventBus: eventBus
        );

        Assert.Equal(26, events.Count);
        Assert.Equal(5, decisionRequestEvents.Count);
        Assert.Equal(new Nickname("BigBlind"), decisionRequestEvents[4].Nickname);

        hand.CommitDecision(
            nickname: new Nickname("BigBlind"),
            decision: new Decision(DecisionType.Fold, new Chips(0)),
            eventBus: eventBus
        );

        Assert.Equal(42, events.Count);
    }

    private Hand CreateHand(
        ImmutableList<Participant> participants,
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
            game: Game.HoldemNoLimit,
            smallBlind: new Chips(smallBlind),
            bigBlind: new Chips(bigBlind),
            maxSeat: new Seat(maxSeat),
            smallBlindSeat: new Seat(smallBlindSeat),
            bigBlindSeat: new Seat(bigBlindSeat),
            buttonSeat: new Seat(buttonSeat),
            participants: participants,
            randomizer: _randomizer,
            evaluator: _evaluator,
            eventBus: new EventBus()
        );
    }

    private Participant CreateParticipant(string nickname, int seat, int stake = 1000)
    {
        return new Participant(
            nickname: new Nickname(nickname),
            seat: new Seat(seat),
            stake: new Chips(stake)
        );
    }
}

public class HoldemNoLimit9MaxHandTest
{
    private readonly FakeRandomizer _randomizer = new();
    private readonly FakeEvaluator _evaluator = new();

    [Fact]
    public void TestFromScratch()
    {
        var handUid = new HandUid(Guid.NewGuid());
        var participantSb = new Participant(
            nickname: new Nickname("SmallBlind"),
            seat: new Seat(1),
            stake: new Chips(1000)
        );
        var participantBb = new Participant(
            nickname: new Nickname("BigBlind"),
            seat: new Seat(2),
            stake: new Chips(900)
        );
        var participantUtg1 = new Participant(
            nickname: new Nickname("UnderTheGun1"),
            seat: new Seat(3),
            stake: new Chips(800)
        );
        var participantUtg2 = new Participant(
            nickname: new Nickname("UnderTheGun2"),
            seat: new Seat(4),
            stake: new Chips(700)
        );
        var participantUtg3 = new Participant(
            nickname: new Nickname("UnderTheGun3"),
            seat: new Seat(5),
            stake: new Chips(600)
        );
        var participantEp = new Participant(
            nickname: new Nickname("Early"),
            seat: new Seat(6),
            stake: new Chips(500)
        );
        var participantMp = new Participant(
            nickname: new Nickname("Middle"),
            seat: new Seat(7),
            stake: new Chips(400)
        );
        var participantCo = new Participant(
            nickname: new Nickname("CutOff"),
            seat: new Seat(8),
            stake: new Chips(300)
        );
        var participantBu = new Participant(
            nickname: new Nickname("Button"),
            seat: new Seat(9),
            stake: new Chips(200)
        );
        var eventBus = new EventBus();

        var events = new List<HandIsCreatedEvent>();
        var listener = (HandIsCreatedEvent @event) => events.Add(@event);
        eventBus.Subscribe(listener);

        var hand = Hand.FromScratch(
            uid: handUid,
            game: Game.HoldemNoLimit,
            smallBlind: new Chips(5),
            bigBlind: new Chips(10),
            maxSeat: new Seat(9),
            smallBlindSeat: new Seat(1),
            bigBlindSeat: new Seat(2),
            buttonSeat: new Seat(9),
            participants: [participantSb, participantBb, participantUtg1, participantUtg2, participantUtg3, participantEp, participantMp, participantCo, participantBu],
            randomizer: _randomizer,
            evaluator: _evaluator,
            eventBus: eventBus
        );

        Assert.Equal(handUid, hand.Uid);
        Assert.Equal(Game.HoldemNoLimit, hand.Game);
        Assert.Equal(new Seat(9), hand.Table.MaxSeat);
        Assert.IsType<NoLimitPot>(hand.Pot);
        Assert.IsType<StandardDeck>(hand.Deck);

        Assert.Single(events);
        Assert.Equal(Game.HoldemNoLimit, events[0].Game);
        Assert.Equal(new Chips(5), events[0].SmallBlind);
        Assert.Equal(new Chips(10), events[0].BigBlind);
        Assert.Equal(9, events[0].Participants.Count);
        Assert.Equal(participantSb, events[0].Participants[0]);
        Assert.Equal(participantBb, events[0].Participants[1]);
        Assert.Equal(participantUtg1, events[0].Participants[2]);
        Assert.Equal(participantUtg2, events[0].Participants[3]);
        Assert.Equal(participantUtg3, events[0].Participants[4]);
        Assert.Equal(participantEp, events[0].Participants[5]);
        Assert.Equal(participantMp, events[0].Participants[6]);
        Assert.Equal(participantCo, events[0].Participants[7]);
        Assert.Equal(participantBu, events[0].Participants[8]);
    }
}

public class OmahaPotLimit6MaxHandTest
{
    private readonly FakeRandomizer _randomizer = new();
    private readonly FakeEvaluator _evaluator = new();

    [Fact]
    public void TestFromScratch()
    {
        var handUid = new HandUid(Guid.NewGuid());
        var participantSb = new Participant(
            nickname: new Nickname("SmallBlind"),
            seat: new Seat(1),
            stake: new Chips(1000)
        );
        var participantBb = new Participant(
            nickname: new Nickname("BigBlind"),
            seat: new Seat(2),
            stake: new Chips(900)
        );
        var participantEp = new Participant(
            nickname: new Nickname("Early"),
            seat: new Seat(3),
            stake: new Chips(800)
        );
        var participantMp = new Participant(
            nickname: new Nickname("Middle"),
            seat: new Seat(4),
            stake: new Chips(700)
        );
        var participantCo = new Participant(
            nickname: new Nickname("CutOff"),
            seat: new Seat(5),
            stake: new Chips(600)
        );
        var participantBu = new Participant(
            nickname: new Nickname("Button"),
            seat: new Seat(6),
            stake: new Chips(500)
        );
        var eventBus = new EventBus();

        var events = new List<HandIsCreatedEvent>();
        var listener = (HandIsCreatedEvent @event) => events.Add(@event);
        eventBus.Subscribe(listener);

        var hand = Hand.FromScratch(
            uid: handUid,
            game: Game.OmahaPotLimit,
            smallBlind: new Chips(5),
            bigBlind: new Chips(10),
            maxSeat: new Seat(6),
            smallBlindSeat: new Seat(1),
            bigBlindSeat: new Seat(2),
            buttonSeat: new Seat(6),
            participants: [participantSb, participantBb, participantEp, participantMp, participantCo, participantBu],
            randomizer: _randomizer,
            evaluator: _evaluator,
            eventBus: eventBus
        );

        Assert.Equal(handUid, hand.Uid);
        Assert.Equal(Game.OmahaPotLimit, hand.Game);
        Assert.Equal(new Seat(6), hand.Table.MaxSeat);
        Assert.IsType<PotLimitPot>(hand.Pot);
        Assert.IsType<StandardDeck>(hand.Deck);

        Assert.Single(events);
        Assert.Equal(Game.OmahaPotLimit, events[0].Game);
        Assert.Equal(new Chips(5), events[0].SmallBlind);
        Assert.Equal(new Chips(10), events[0].BigBlind);
        Assert.Equal(6, events[0].Participants.Count);
        Assert.Equal(participantSb, events[0].Participants[0]);
        Assert.Equal(participantBb, events[0].Participants[1]);
        Assert.Equal(participantEp, events[0].Participants[2]);
        Assert.Equal(participantMp, events[0].Participants[3]);
        Assert.Equal(participantCo, events[0].Participants[4]);
        Assert.Equal(participantBu, events[0].Participants[5]);
    }
}

public class OmahaPotLimit9MaxHandTest
{
    private readonly FakeRandomizer _randomizer = new();
    private readonly FakeEvaluator _evaluator = new();

    [Fact]
    public void TestFromScratch()
    {
        var handUid = new HandUid(Guid.NewGuid());
        var participantSb = new Participant(
            nickname: new Nickname("SmallBlind"),
            seat: new Seat(1),
            stake: new Chips(1000)
        );
        var participantBb = new Participant(
            nickname: new Nickname("BigBlind"),
            seat: new Seat(2),
            stake: new Chips(900)
        );
        var participantUtg1 = new Participant(
            nickname: new Nickname("UnderTheGun1"),
            seat: new Seat(3),
            stake: new Chips(800)
        );
        var participantUtg2 = new Participant(
            nickname: new Nickname("UnderTheGun2"),
            seat: new Seat(4),
            stake: new Chips(700)
        );
        var participantUtg3 = new Participant(
            nickname: new Nickname("UnderTheGun3"),
            seat: new Seat(5),
            stake: new Chips(600)
        );
        var participantEp = new Participant(
            nickname: new Nickname("Early"),
            seat: new Seat(6),
            stake: new Chips(500)
        );
        var participantMp = new Participant(
            nickname: new Nickname("Middle"),
            seat: new Seat(7),
            stake: new Chips(400)
        );
        var participantCo = new Participant(
            nickname: new Nickname("CutOff"),
            seat: new Seat(8),
            stake: new Chips(300)
        );
        var participantBu = new Participant(
            nickname: new Nickname("Button"),
            seat: new Seat(9),
            stake: new Chips(200)
        );
        var eventBus = new EventBus();

        var events = new List<HandIsCreatedEvent>();
        var listener = (HandIsCreatedEvent @event) => events.Add(@event);
        eventBus.Subscribe(listener);

        var hand = Hand.FromScratch(
            uid: handUid,
            game: Game.OmahaPotLimit,
            smallBlind: new Chips(5),
            bigBlind: new Chips(10),
            maxSeat: new Seat(9),
            smallBlindSeat: new Seat(1),
            bigBlindSeat: new Seat(2),
            buttonSeat: new Seat(9),
            participants: [participantSb, participantBb, participantUtg1, participantUtg2, participantUtg3, participantEp, participantMp, participantCo, participantBu],
            randomizer: _randomizer,
            evaluator: _evaluator,
            eventBus: eventBus
        );

        Assert.Equal(handUid, hand.Uid);
        Assert.Equal(Game.OmahaPotLimit, hand.Game);
        Assert.Equal(new Seat(9), hand.Table.MaxSeat);
        Assert.IsType<PotLimitPot>(hand.Pot);
        Assert.IsType<StandardDeck>(hand.Deck);

        Assert.Single(events);
        Assert.Equal(Game.OmahaPotLimit, events[0].Game);
        Assert.Equal(new Chips(5), events[0].SmallBlind);
        Assert.Equal(new Chips(10), events[0].BigBlind);
        Assert.Equal(9, events[0].Participants.Count);
        Assert.Equal(participantSb, events[0].Participants[0]);
        Assert.Equal(participantBb, events[0].Participants[1]);
        Assert.Equal(participantUtg1, events[0].Participants[2]);
        Assert.Equal(participantUtg2, events[0].Participants[3]);
        Assert.Equal(participantUtg3, events[0].Participants[4]);
        Assert.Equal(participantEp, events[0].Participants[5]);
        Assert.Equal(participantMp, events[0].Participants[6]);
        Assert.Equal(participantCo, events[0].Participants[7]);
        Assert.Equal(participantBu, events[0].Participants[8]);
    }
}
