namespace Application.IntegrationEvent;

public record BlindPostedIntegrationEvent : IIntegrationEvent
{
    public required Guid Uid { init; get; }
    public Guid? CorrelationUid { init; get; }
    public required DateTime OccurredAt { get; init; }

    public required Guid HandUid { get; init; }
    public required Guid TableUid { get; init; }
    public required string TableType { get; init; }

    public required string Nickname { get; init; }
    public required int Amount { get; init; }
}
