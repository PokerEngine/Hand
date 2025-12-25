namespace Application.IntegrationEvent;

public record struct BoardCardsAreDealtIntegrationEvent : IIntegrationEvent
{
    public required Guid HandUid { get; init; }
    public required string Cards { get; init; }
    public required DateTime OccuredAt { get; init; }
}
