namespace Application;

public record struct ParticipantDto
{
    public required string Nickname { get; init; }
    public required int Seat { get; init; }
    public required int Stack { get; init; }
}
