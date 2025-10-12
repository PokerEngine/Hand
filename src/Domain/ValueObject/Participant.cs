namespace Domain.ValueObject;

public readonly struct Participant
{
    public readonly Nickname Nickname;
    public readonly Seat Seat;
    public readonly Chips Stack;

    public Participant(Nickname nickname, Seat seat, Chips stack)
    {
        Nickname = nickname;
        Seat = seat;
        Stack = stack;
    }

    public override string ToString() =>
        $"{Nickname}, {Seat}, {Stack}";
}
