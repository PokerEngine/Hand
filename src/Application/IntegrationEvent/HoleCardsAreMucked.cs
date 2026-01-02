namespace Application.IntegrationEvent;

public record struct HoleCardsAreMuckedIntegrationEvent : IIntegrationEvent
{
    public required Guid HandUid { get; init; }
    public required Guid TableUid { get; init; }
    public required string TableType { get; init; }
    public required string Nickname { get; init; }
    public required DateTime OccurredAt { get; init; }
}
