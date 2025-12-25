namespace Application.IntegrationEvent;

public record struct HandIsFinishedIntegrationEvent : IIntegrationEvent
{
    public required Guid HandUid { get; init; }
    public required DateTime OccuredAt { get; init; }
}
