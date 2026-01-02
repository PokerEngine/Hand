namespace Application.IntegrationEvent;

public record struct HandIsCreatedIntegrationEvent : IIntegrationEvent
{
    public required Guid HandUid { get; init; }
    public required Guid TableUid { get; init; }
    public required string TableType { get; init; }
    public required string Game { get; init; }
    public required int MaxSeat { get; init; }
    public required int SmallBlind { get; init; }
    public required int BigBlind { get; init; }
    public required int SmallBlindSeat { get; init; }
    public required int BigBlindSeat { get; init; }
    public required int ButtonSeat { get; init; }
    public required List<ParticipantDto> Participants { get; init; }
    public required DateTime OccurredAt { get; init; }
}
