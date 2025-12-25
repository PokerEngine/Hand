namespace Application.IntegrationEvent;

public record struct BlindIsPostedIntegrationEvent : IIntegrationEvent
{
    public required Guid HandUid { get; init; }
    public required string Nickname { get; init; }
    public required int Amount { get; init; }
    public required DateTime OccuredAt { get; init; }
}
