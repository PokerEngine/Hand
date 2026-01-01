using Domain.ValueObject;

namespace Application.Event;

public record struct EventContext
{
    public required HandUid HandUid { get; init; }
    public required HandType HandType { get; init; }
}
