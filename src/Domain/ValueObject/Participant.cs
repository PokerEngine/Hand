namespace Domain.ValueObject;

public readonly struct Participant
{
    public readonly Nickname Nickname;
    public readonly Seat Seat;
    public readonly Chips Stake;

    public Participant(Nickname nickname, Seat seat, Chips stake)
    {
        Nickname = nickname;
        Seat = seat;
        Stake = stake;
    }

    public override string ToString() =>
        $"{Nickname}, {Seat}, {Stake}";
}
