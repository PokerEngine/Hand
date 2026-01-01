namespace Application.IntegrationEvent;

public record struct HoleCardsAreShownIntegrationEvent : IIntegrationEvent
{
    public required Guid HandUid { get; init; }
    public required Guid TableUid { get; init; }
    public required string TableType { get; init; }
    public required string Nickname { get; init; }
    public required string Cards { get; init; }
    public required string ComboType { get; init; }
    public required int ComboWeight { get; init; }
    public required DateTime OccuredAt { get; init; }
}
