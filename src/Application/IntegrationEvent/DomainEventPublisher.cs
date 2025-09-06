using Domain.Event;
using Domain.ValueObject;
using System.Collections.Immutable;

namespace Application.IntegrationEvent;

public class DomainEventPublisher
{
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly Guid _tableUid;
    private readonly Guid _handUid;

    public DomainEventPublisher(
        IIntegrationEventBus integrationEventBus,
        Guid tableUid,
        Guid handUid
    )
    {
        _integrationEventBus = integrationEventBus;
        _tableUid = tableUid;
        _handUid = handUid;
    }

    public async Task Publish(IEnumerable<BaseEvent> events)
    {
        foreach (var @event in events)
        {
            await PublishEvent((dynamic)@event);
        }
    }

    private async Task PublishEvent(HandIsCreatedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.hand-created");
        var integrationEvent = new HandIsCreatedIntegrationEvent(
            Game: @event.Game.ToString(),
            SmallBlind: @event.SmallBlind,
            BigBlind: @event.BigBlind,
            MaxSeat: @event.MaxSeat,
            SmallBlindSeat: @event.SmallBlindSeat,
            BigBlindSeat: @event.BigBlindSeat,
            ButtonSeat: @event.ButtonSeat,
            Participants: @event.Participants.Select(ProcessParticipant).ToImmutableList(),
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        await _integrationEventBus.Publish(integrationEvent, queue);
    }

    private async Task PublishEvent(HandIsStartedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.hand-started");
        var integrationEvent = new HandIsStartedIntegrationEvent(
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        await _integrationEventBus.Publish(integrationEvent, queue);
    }

    private async Task PublishEvent(HandIsFinishedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.hand-finished");
        var integrationEvent = new HandIsFinishedIntegrationEvent(
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        await _integrationEventBus.Publish(integrationEvent, queue);
    }

    private async Task PublishEvent(PlayerConnectedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.player-connected");
        var integrationEvent = new PlayerConnectedIntegrationEvent(
            Nickname: @event.Nickname,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        await _integrationEventBus.Publish(integrationEvent, queue);
    }

    private async Task PublishEvent(PlayerDisconnectedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.player-disconnected");
        var integrationEvent = new PlayerDisconnectedIntegrationEvent(
            Nickname: @event.Nickname,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        await _integrationEventBus.Publish(integrationEvent, queue);
    }

    private async Task PublishEvent(SmallBlindIsPostedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.blind-posted");
        var integrationEvent = new BlindIsPostedIntegrationEvent(
            Nickname: @event.Nickname,
            Amount: @event.Amount,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        await _integrationEventBus.Publish(integrationEvent, queue);
    }

    private async Task PublishEvent(BigBlindIsPostedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.blind-posted");
        var integrationEvent = new BlindIsPostedIntegrationEvent(
            Nickname: @event.Nickname,
            Amount: @event.Amount,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        await _integrationEventBus.Publish(integrationEvent, queue);
    }

    private async Task PublishEvent(HoleCardsAreDealtEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.hole-cards-dealt");
        var integrationEvent = new HoleCardsAreDealtIntegrationEvent(
            Nickname: @event.Nickname,
            Cards: @event.Cards.ToString(),
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        await _integrationEventBus.Publish(integrationEvent, queue);
    }

    private async Task PublishEvent(BoardCardsAreDealtEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.board-cards-dealt");
        var integrationEvent = new BoardCardsAreDealtIntegrationEvent(
            Cards: @event.Cards.ToString(),
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        await _integrationEventBus.Publish(integrationEvent, queue);
    }

    private async Task PublishEvent(DecisionIsRequestedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.decision-requested");
        var integrationEvent = new DecisionIsRequestedIntegrationEvent(
            Nickname: @event.Nickname,
            FoldIsAvailable: @event.FoldIsAvailable,
            CheckIsAvailable: @event.CheckIsAvailable,
            CallIsAvailable: @event.CallIsAvailable,
            CallToAmount: @event.CallToAmount,
            RaiseIsAvailable: @event.RaiseIsAvailable,
            MinRaiseToAmount: @event.MinRaiseToAmount,
            MaxRaiseToAmount: @event.MaxRaiseToAmount,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        await _integrationEventBus.Publish(integrationEvent, queue);
    }

    private async Task PublishEvent(DecisionIsCommittedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.decision-committed");
        var integrationEvent = new DecisionIsCommittedIntegrationEvent(
            Nickname: @event.Nickname,
            DecisionType: @event.Decision.Type.ToString(),
            DecisionAmount: @event.Decision.Amount,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        await _integrationEventBus.Publish(integrationEvent, queue);
    }

    private async Task PublishEvent(RefundIsCommittedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.refund-committed");
        var integrationEvent = new RefundIsCommittedIntegrationEvent(
            Nickname: @event.Nickname,
            Amount: @event.Amount,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        await _integrationEventBus.Publish(integrationEvent, queue);
    }

    private async Task PublishEvent(WinWithoutShowdownIsCommittedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.win-without-showdown-committed");
        var integrationEvent = new WinWithoutShowdownIsCommittedIntegrationEvent(
            Nickname: @event.Nickname,
            Amount: @event.Amount,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        await _integrationEventBus.Publish(integrationEvent, queue);
    }

    private async Task PublishEvent(WinAtShowdownIsCommittedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.win-at-showdown-committed");
        foreach (var (nickname, amount) in @event.WinPot)
        {
            var integrationEvent = new WinAtShowdownIsCommittedIntegrationEvent(
                Nickname: nickname,
                Amount: amount,
                HandUid: _handUid,
                TableUid: _tableUid,
                OccuredAt: @event.OccuredAt
            );
            await _integrationEventBus.Publish(integrationEvent, queue);
        }
    }

    private async Task PublishEvent(HoleCardsAreShownEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.hole-cards-shown");
        var integrationEvent = new HoleCardsAreShownIntegrationEvent(
            Nickname: @event.Nickname,
            Cards: @event.Cards.ToString(),
            ComboType: @event.Combo.Type.ToString(),
            ComboWeight: @event.Combo.Weight,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        await _integrationEventBus.Publish(integrationEvent, queue);
    }

    private async Task PublishEvent(HoleCardsAreMuckedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.hole-cards-mucked");
        var integrationEvent = new HoleCardsAreMuckedIntegrationEvent(
            Nickname: @event.Nickname,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        await _integrationEventBus.Publish(integrationEvent, queue);
    }

    private async Task PublishEvent(object @event)
    {
        // No handler for the given event
        await Task.CompletedTask;
    }

    private IntegrationEventParticipant ProcessParticipant(Participant participant)
    {
        return new IntegrationEventParticipant(
            Nickname: participant.Nickname,
            Seat: participant.Seat,
            Stake: participant.Stake
        );
    }
}
