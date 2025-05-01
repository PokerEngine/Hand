using Domain.Entity;
using Domain.Event;
using Domain.ValueObject;

namespace DomainTest.EntityTest;

public class HoldemNoLimit6MaxHandTest
{
    [Fact]
    public void TestFromScratch()
    {
        var handUid = new HandUid(Guid.NewGuid());
        var participantSb = new Participant(
            nickname: new Nickname("SmallBlind"),
            position: Position.SmallBlind,
            stake: new Chips(1000)
        );
        var participantBb = new Participant(
            nickname: new Nickname("BigBlind"),
            position: Position.BigBlind,
            stake: new Chips(900)
        );
        var participantEp = new Participant(
            nickname: new Nickname("Early"),
            position: Position.Early,
            stake: new Chips(800)
        );
        var participantMp = new Participant(
            nickname: new Nickname("Middle"),
            position: Position.Middle,
            stake: new Chips(700)
        );
        var participantCo = new Participant(
            nickname: new Nickname("CutOff"),
            position: Position.CutOff,
            stake: new Chips(600)
        );
        var participantBu = new Participant(
            nickname: new Nickname("Button"),
            position: Position.Button,
            stake: new Chips(500)
        );
        var eventBus = new EventBus();

        var events = new List<HandIsCreatedEvent>();
        var listener = (HandIsCreatedEvent @event) => events.Add(@event);
        eventBus.Subscribe(listener);

        var hand = Hand.FromScratch(
            uid: handUid,
            game: Game.HoldemNoLimit6Max,
            smallBlind: new Chips(5),
            bigBlind: new Chips(10),
            participants: [participantSb, participantBb, participantEp, participantMp, participantCo, participantBu],
            eventBus: eventBus
        );

        Assert.Equal(handUid, hand.Uid);
        Assert.Equal(Game.HoldemNoLimit6Max, hand.Game);
        Assert.IsType<SixMaxTable>(hand.Table);
        Assert.IsType<NoLimitPot>(hand.Pot);
        Assert.IsType<StandardDeck>(hand.Deck);

        Assert.Single(events);
        Assert.Equal(Game.HoldemNoLimit6Max, events[0].Game);
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
            position: Position.SmallBlind,
            stake: new Chips(1000)
        );
        var participantBb = new Participant(
            nickname: new Nickname("BigBlind"),
            position: Position.BigBlind,
            stake: new Chips(900)
        );
        var participantBu = new Participant(
            nickname: new Nickname("Button"),
            position: Position.Button,
            stake: new Chips(800)
        );

        var events = new List<IEvent>
        {
            new HandIsCreatedEvent(
                Game: Game.HoldemNoLimit6Max,
                SmallBlind: new Chips(5),
                BigBlind: new Chips(10),
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

        var hand = Hand.FromEvents(uid: handUid, events: events);

        Assert.Equal(handUid, hand.Uid);
        Assert.Equal(Game.HoldemNoLimit6Max, hand.Game);
        Assert.IsType<SixMaxTable>(hand.Table);
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
            position: Position.SmallBlind
        );
        var participantBb = CreateParticipant(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var participantBu = CreateParticipant(
            nickname: "Button",
            position: Position.Button
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
            position: Position.SmallBlind
        );
        var participantBb = CreateParticipant(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var participantBu = CreateParticipant(
            nickname: "Button",
            position: Position.Button
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
            position: Position.SmallBlind
        );
        var participantBb = CreateParticipant(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var participantBu = CreateParticipant(
            nickname: "Button",
            position: Position.Button
        );
        var eventBus = new EventBus();

        var hand = CreateHand(
            participants: [participantSb, participantBb, participantBu]
        );

        var events = new List<IEvent>();
        var decisionRequestEvents = new List<DecisionIsRequestedEvent>();
        var listener = (IEvent @event) => events.Add(@event);
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

    private Hand CreateHand(List<Participant> participants, int smallBlind = 5, int bigBlind = 10)
    {
        return Hand.FromScratch(
            uid: new HandUid(Guid.NewGuid()),
            game: Game.HoldemNoLimit6Max,
            smallBlind: new Chips(smallBlind),
            bigBlind: new Chips(bigBlind),
            participants: participants,
            eventBus: new EventBus()
        );
    }

    private Participant CreateParticipant(string nickname, Position position, int stake = 1000)
    {
        return new Participant(
            nickname: new Nickname(nickname),
            position: position,
            stake: new Chips(stake)
        );
    }
}

public class HoldemNoLimit9MaxHandTest
{
    [Fact]
    public void TestFromScratch()
    {
        var handUid = new HandUid(Guid.NewGuid());
        var participantSb = new Participant(
            nickname: new Nickname("SmallBlind"),
            position: Position.SmallBlind,
            stake: new Chips(1000)
        );
        var participantBb = new Participant(
            nickname: new Nickname("BigBlind"),
            position: Position.BigBlind,
            stake: new Chips(900)
        );
        var participantUtg1 = new Participant(
            nickname: new Nickname("UnderTheGun1"),
            position: Position.UnderTheGun1,
            stake: new Chips(800)
        );
        var participantUtg2 = new Participant(
            nickname: new Nickname("UnderTheGun2"),
            position: Position.UnderTheGun2,
            stake: new Chips(700)
        );
        var participantUtg3 = new Participant(
            nickname: new Nickname("UnderTheGun3"),
            position: Position.UnderTheGun3,
            stake: new Chips(600)
        );
        var participantEp = new Participant(
            nickname: new Nickname("Early"),
            position: Position.Early,
            stake: new Chips(500)
        );
        var participantMp = new Participant(
            nickname: new Nickname("Middle"),
            position: Position.Middle,
            stake: new Chips(400)
        );
        var participantCo = new Participant(
            nickname: new Nickname("CutOff"),
            position: Position.CutOff,
            stake: new Chips(300)
        );
        var participantBu = new Participant(
            nickname: new Nickname("Button"),
            position: Position.Button,
            stake: new Chips(200)
        );
        var eventBus = new EventBus();

        var events = new List<HandIsCreatedEvent>();
        var listener = (HandIsCreatedEvent @event) => events.Add(@event);
        eventBus.Subscribe(listener);

        var hand = Hand.FromScratch(
            uid: handUid,
            game: Game.HoldemNoLimit9Max,
            smallBlind: new Chips(5),
            bigBlind: new Chips(10),
            participants: [participantSb, participantBb, participantUtg1, participantUtg2, participantUtg3, participantEp, participantMp, participantCo, participantBu],
            eventBus: eventBus
        );

        Assert.Equal(handUid, hand.Uid);
        Assert.Equal(Game.HoldemNoLimit9Max, hand.Game);
        Assert.IsType<NineMaxTable>(hand.Table);
        Assert.IsType<NoLimitPot>(hand.Pot);
        Assert.IsType<StandardDeck>(hand.Deck);

        Assert.Single(events);
        Assert.Equal(Game.HoldemNoLimit9Max, events[0].Game);
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
    [Fact]
    public void TestFromScratch()
    {
        var handUid = new HandUid(Guid.NewGuid());
        var participantSb = new Participant(
            nickname: new Nickname("SmallBlind"),
            position: Position.SmallBlind,
            stake: new Chips(1000)
        );
        var participantBb = new Participant(
            nickname: new Nickname("BigBlind"),
            position: Position.BigBlind,
            stake: new Chips(900)
        );
        var participantEp = new Participant(
            nickname: new Nickname("Early"),
            position: Position.Early,
            stake: new Chips(800)
        );
        var participantMp = new Participant(
            nickname: new Nickname("Middle"),
            position: Position.Middle,
            stake: new Chips(700)
        );
        var participantCo = new Participant(
            nickname: new Nickname("CutOff"),
            position: Position.CutOff,
            stake: new Chips(600)
        );
        var participantBu = new Participant(
            nickname: new Nickname("Button"),
            position: Position.Button,
            stake: new Chips(500)
        );
        var eventBus = new EventBus();

        var events = new List<HandIsCreatedEvent>();
        var listener = (HandIsCreatedEvent @event) => events.Add(@event);
        eventBus.Subscribe(listener);

        var hand = Hand.FromScratch(
            uid: handUid,
            game: Game.OmahaPotLimit6Max,
            smallBlind: new Chips(5),
            bigBlind: new Chips(10),
            participants: [participantSb, participantBb, participantEp, participantMp, participantCo, participantBu],
            eventBus: eventBus
        );

        Assert.Equal(handUid, hand.Uid);
        Assert.Equal(Game.OmahaPotLimit6Max, hand.Game);
        Assert.IsType<SixMaxTable>(hand.Table);
        Assert.IsType<PotLimitPot>(hand.Pot);
        Assert.IsType<StandardDeck>(hand.Deck);

        Assert.Single(events);
        Assert.Equal(Game.OmahaPotLimit6Max, events[0].Game);
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
    [Fact]
    public void TestFromScratch()
    {
        var handUid = new HandUid(Guid.NewGuid());
        var participantSb = new Participant(
            nickname: new Nickname("SmallBlind"),
            position: Position.SmallBlind,
            stake: new Chips(1000)
        );
        var participantBb = new Participant(
            nickname: new Nickname("BigBlind"),
            position: Position.BigBlind,
            stake: new Chips(900)
        );
        var participantUtg1 = new Participant(
            nickname: new Nickname("UnderTheGun1"),
            position: Position.UnderTheGun1,
            stake: new Chips(800)
        );
        var participantUtg2 = new Participant(
            nickname: new Nickname("UnderTheGun2"),
            position: Position.UnderTheGun2,
            stake: new Chips(700)
        );
        var participantUtg3 = new Participant(
            nickname: new Nickname("UnderTheGun3"),
            position: Position.UnderTheGun3,
            stake: new Chips(600)
        );
        var participantEp = new Participant(
            nickname: new Nickname("Early"),
            position: Position.Early,
            stake: new Chips(500)
        );
        var participantMp = new Participant(
            nickname: new Nickname("Middle"),
            position: Position.Middle,
            stake: new Chips(400)
        );
        var participantCo = new Participant(
            nickname: new Nickname("CutOff"),
            position: Position.CutOff,
            stake: new Chips(300)
        );
        var participantBu = new Participant(
            nickname: new Nickname("Button"),
            position: Position.Button,
            stake: new Chips(200)
        );
        var eventBus = new EventBus();

        var events = new List<HandIsCreatedEvent>();
        var listener = (HandIsCreatedEvent @event) => events.Add(@event);
        eventBus.Subscribe(listener);

        var hand = Hand.FromScratch(
            uid: handUid,
            game: Game.OmahaPotLimit9Max,
            smallBlind: new Chips(5),
            bigBlind: new Chips(10),
            participants: [participantSb, participantBb, participantUtg1, participantUtg2, participantUtg3, participantEp, participantMp, participantCo, participantBu],
            eventBus: eventBus
        );

        Assert.Equal(handUid, hand.Uid);
        Assert.Equal(Game.OmahaPotLimit9Max, hand.Game);
        Assert.IsType<NineMaxTable>(hand.Table);
        Assert.IsType<PotLimitPot>(hand.Pot);
        Assert.IsType<StandardDeck>(hand.Deck);

        Assert.Single(events);
        Assert.Equal(Game.OmahaPotLimit9Max, events[0].Game);
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
