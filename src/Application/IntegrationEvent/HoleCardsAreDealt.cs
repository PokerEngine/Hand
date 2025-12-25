namespace Application.IntegrationEvent;

public record struct HoleCardsAreDealtIntegrationEvent : IIntegrationEvent
{
    public required Guid HandUid { get; init; }
    public required string Nickname { get; init; }
    public required string Cards { get; init; }
    public required DateTime OccuredAt { get; init; }
}
