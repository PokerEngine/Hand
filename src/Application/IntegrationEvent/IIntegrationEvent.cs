namespace Application.IntegrationEvent;

public interface IIntegrationEvent
{
    Guid HandUid { init; get; }
    DateTime OccuredAt { init; get; }
}
