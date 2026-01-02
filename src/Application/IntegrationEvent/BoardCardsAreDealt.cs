namespace Application.IntegrationEvent;

public record struct BoardCardsAreDealtIntegrationEvent : IIntegrationEvent
{
    public required Guid HandUid { get; init; }
    public required Guid TableUid { get; init; }
    public required string TableType { get; init; }
    public required string Cards { get; init; }
    public required DateTime OccurredAt { get; init; }
}
