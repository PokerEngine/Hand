namespace Domain.ValueObject;

public readonly struct Participant
{
    public readonly Nickname Nickname;
    public readonly Position Position;
    public readonly Chips Stake;

    public Participant(Nickname nickname, Position position, Chips stake)
    {
        Nickname = nickname;
        Position = position;
        Stake = stake;
    }

    public override string ToString() =>
        $"{Nickname}, {Position}, {Stake}";
}
