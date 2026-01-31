namespace Application.IntegrationEvent;

public interface IIntegrationEvent
{
    Guid Uid { init; get; }
    Guid? CorrelationUid { init; get; }
    DateTime OccurredAt { init; get; }

    Guid HandUid { init; get; }
    Guid TableUid { init; get; }
    string TableType { init; get; }
}
