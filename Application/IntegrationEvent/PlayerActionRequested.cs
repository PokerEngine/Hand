namespace Application.IntegrationEvent;

public record PlayerActionRequestedIntegrationEvent : IIntegrationEvent
{
    public required Guid Uid { init; get; }
    public Guid? CorrelationUid { init; get; }
    public required DateTime OccurredAt { get; init; }

    public required Guid HandUid { get; init; }
    public required Guid TableUid { get; init; }
    public required string TableType { get; init; }

    public required string Nickname { get; init; }
    public required bool FoldIsAvailable { get; init; }
    public required bool CheckIsAvailable { get; init; }
    public required bool CallIsAvailable { get; init; }
    public required int CallByAmount { get; init; }
    public required bool RaiseIsAvailable { get; init; }
    public required int MinRaiseByAmount { get; init; }
    public required int MaxRaiseByAmount { get; init; }
}
