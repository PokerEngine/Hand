namespace Application.IntegrationEvent;

public interface IIntegrationEvent
{
    Guid HandUid { init; get; }
    Guid TableUid { init; get; }
    string TableType { init; get; }
    DateTime OccuredAt { init; get; }
}
