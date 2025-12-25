namespace Application.IntegrationEvent;

public record struct DecisionIsCommittedIntegrationEvent : IIntegrationEvent
{
    public required Guid HandUid { get; init; }
    public required string Nickname { get; init; }
    public required string DecisionType { get; init; }
    public required int DecisionAmount { get; init; }
    public required DateTime OccuredAt { get; init; }
}
