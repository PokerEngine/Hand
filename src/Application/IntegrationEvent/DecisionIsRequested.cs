namespace Application.IntegrationEvent;

public record struct DecisionIsRequestedIntegrationEvent : IIntegrationEvent
{
    public required Guid HandUid { get; init; }
    public required string Nickname { get; init; }
    public required bool FoldIsAvailable { get; init; }
    public required bool CheckIsAvailable { get; init; }
    public required bool CallIsAvailable { get; init; }
    public required int CallToAmount { get; init; }
    public required bool RaiseIsAvailable { get; init; }
    public required int MinRaiseToAmount { get; init; }
    public required int MaxRaiseToAmount { get; init; }
    public required DateTime OccuredAt { get; init; }
}
