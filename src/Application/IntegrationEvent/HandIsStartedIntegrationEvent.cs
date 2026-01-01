namespace Application.IntegrationEvent;

public record struct HandIsStartedIntegrationEvent : IIntegrationEvent
{
    public required Guid HandUid { get; init; }
    public required Guid TableUid { get; init; }
    public required string TableType { get; init; }
    public required DateTime OccuredAt { get; init; }
}
