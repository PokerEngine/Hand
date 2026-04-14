using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class HandStartedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HandStartedEvent>
{
    public async Task HandleAsync(HandStartedEvent @event)
    {
        var integrationEvent = new HandStartedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = @event.HandUid,
            TableUid = @event.TableContext.TableUid,
            TableType = @event.TableContext.TableType.ToString(),
            Game = @event.Rules.Game.ToString(),
            MaxSeat = @event.Rules.MaxSeat,
            SmallBlind = @event.Rules.SmallBlind,
            BigBlind = @event.Rules.BigBlind,
            SmallBlindSeat = @event.Positions.SmallBlindSeat,
            BigBlindSeat = @event.Positions.BigBlindSeat,
            ButtonSeat = @event.Positions.ButtonSeat,
            Players = @event.Players.Select(SerializeParticipant).ToList()
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{@event.TableContext.TableType.ToRoutingKey()}.hand-created");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }

    private IntegrationEventParticipant SerializeParticipant(Participant participant)
    {
        return new IntegrationEventParticipant
        {
            Nickname = participant.Nickname,
            Seat = participant.Seat,
            Stack = participant.Stack
        };
    }
}
