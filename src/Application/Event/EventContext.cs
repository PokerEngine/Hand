using Domain.ValueObject;

namespace Application.Event;

public record struct EventContext
{
    public required HandUid HandUid { get; init; }
    public required TableUid TableUid { get; init; }
    public required TableType TableType { get; init; }
}
