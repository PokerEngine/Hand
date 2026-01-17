namespace Domain.ValueObject;

public struct Participant
{
    public Nickname Nickname { get; init; }
    public Seat Seat { get; init; }
    public Chips Stack { get; init; }
}
