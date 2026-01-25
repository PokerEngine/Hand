using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class HandCreatedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HandCreatedEvent>
{
    public async Task HandleAsync(HandCreatedEvent @event, EventContext context)
    {
        var integrationEvent = new HandCreatedIntegrationEvent
        {
            Uid = Guid.NewGuid(),
            OccurredAt = @event.OccurredAt,
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Game = @event.Rules.Game.ToString(),
            SmallBlind = @event.Rules.SmallBlind,
            BigBlind = @event.Rules.BigBlind,
            MaxSeat = @event.Positions.Max,
            SmallBlindSeat = @event.Positions.SmallBlind,
            BigBlindSeat = @event.Positions.BigBlind,
            ButtonSeat = @event.Positions.Button,
            Participants = @event.Participants.Select(SerializeParticipant).ToList()
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.hand-created");
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
