namespace Domain.ValueObject;

public record Participant
{
    public Nickname Nickname { get; init; }
    public Seat Seat { get; init; }
    public Chips Stack { get; init; }
}
