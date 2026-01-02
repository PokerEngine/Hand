using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class HandIsCreatedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HandIsCreatedEvent>
{
    public async Task HandleAsync(HandIsCreatedEvent @event, EventContext context)
    {
        var integrationEvent = new HandIsCreatedIntegrationEvent
        {
            HandUid = context.HandUid,
            TableUid = context.TableUid,
            TableType = context.TableType.ToString(),
            Game = @event.Game.ToString(),
            MaxSeat = @event.MaxSeat,
            SmallBlind = @event.SmallBlind,
            BigBlind = @event.BigBlind,
            SmallBlindSeat = @event.SmallBlindSeat,
            BigBlindSeat = @event.BigBlindSeat,
            ButtonSeat = @event.ButtonSeat,
            Participants = @event.Participants.Select(SerializeParticipant).ToList(),
            OccurredAt = @event.OccurredAt
        };

        var routingKey = new IntegrationEventRoutingKey($"hand.{context.TableType.ToRoutingKey()}.hand-is-created");
        await integrationEventPublisher.PublishAsync(integrationEvent, routingKey);
    }

    private ParticipantDto SerializeParticipant(Participant participant)
    {
        return new ParticipantDto
        {
            Nickname = participant.Nickname,
            Seat = participant.Seat,
            Stack = participant.Stack
        };
    }
}
