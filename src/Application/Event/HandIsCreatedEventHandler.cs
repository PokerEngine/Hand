using Application.IntegrationEvent;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Event;

public class HandIsCreatedEventHandler(
    IIntegrationEventPublisher integrationEventPublisher
) : IEventHandler<HandIsCreatedEvent>
{
    public async Task HandleAsync(HandIsCreatedEvent @event, HandUid handUid)
    {
        var integrationEvent = new HandIsCreatedIntegrationEvent
        {
            HandUid = handUid,
            Type = @event.Type.ToString(),
            Game = @event.Game.ToString(),
            MaxSeat = @event.MaxSeat,
            SmallBlind = @event.SmallBlind,
            BigBlind = @event.BigBlind,
            SmallBlindSeat = @event.SmallBlindSeat,
            BigBlindSeat = @event.BigBlindSeat,
            ButtonSeat = @event.ButtonSeat,
            Participants = @event.Participants.Select(SerializeParticipant).ToList(),
            OccuredAt = @event.OccuredAt
        };

        await integrationEventPublisher.PublishAsync(integrationEvent, "hand.hand-is-created");
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
