namespace Application.IntegrationEvent;

public record struct HoleCardsAreMuckedIntegrationEvent : IIntegrationEvent
{
    public required Guid HandUid { get; init; }
    public required string Nickname { get; init; }
    public required DateTime OccuredAt { get; init; }
}
