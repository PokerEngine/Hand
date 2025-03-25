using Domain.Entity;
using Domain.Entity.Factory;
using Domain.Event;
using Domain.ValueObject;

namespace DomainTest.EntityTest;

public class HoldemNoLimit6MaxHandTest
{
    [Fact]
    public void TestInitialization()
    {
        var factory = new HoldemNoLimit6MaxFactory();

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

        var hand = factory.Build(
            handUid: new HandUid(Guid.NewGuid()),
            smallBlind: new Chips(5),
            bigBlind: new Chips(10),
            participants: [participantSb, participantBb, participantBu]
        );

        Assert.Equal(Game.HoldemNoLimit6Max, hand.Game);
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
        var hand = CreateHand(
            participants: [participantSb, participantBb, participantBu]
        );
        var eventBus = new EventBus();

        var events = new List<PlayerConnectedEvent>();
        var listener = (PlayerConnectedEvent @event) => events.Add(@event);
        eventBus.Subscribe(listener);

        hand.Connect(
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
        var hand = CreateHand(
            participants: [participantSb, participantBb, participantBu]
        );
        var eventBus = new EventBus();

        var events = new List<PlayerDisconnectedEvent>();
        var listener = (PlayerDisconnectedEvent @event) => events.Add(@event);
        eventBus.Subscribe(listener);

        hand.Connect(
            nickname: new Nickname("SmallBlind"),
            eventBus: eventBus
        );

        hand.Disconnect(
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
        var hand = CreateHand(
            participants: [participantSb, participantBb, participantBu]
        );
        var eventBus = new EventBus();

        var events = new List<BaseEvent>();
        var decisionRequestEvents = new List<DecisionIsRequestedEvent>();
        var listener = (BaseEvent @event) => events.Add(@event);
        var decisionRequestListener = (DecisionIsRequestedEvent @event) => decisionRequestEvents.Add(@event);
        eventBus.Subscribe(listener);
        eventBus.Subscribe(decisionRequestListener);

        hand.Connect(
            nickname: new Nickname("SmallBlind"),
            eventBus: eventBus
        );
        hand.Connect(
            nickname: new Nickname("BigBlind"),
            eventBus: eventBus
        );
        hand.Connect(
            nickname: new Nickname("Button"),
            eventBus: eventBus
        );

        hand.Start(eventBus);

        Assert.Equal(15, events.Count);
        Assert.Equal(1, decisionRequestEvents.Count);
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

    private Hand CreateHand(IEnumerable<Participant> participants, int smallBlind = 5, int bigBlind = 10)
    {
        var factory = new HoldemNoLimit6MaxFactory();

        return factory.Build(
            handUid: new HandUid(Guid.NewGuid()),
            smallBlind: new Chips(smallBlind),
            bigBlind: new Chips(bigBlind),
            participants: participants
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