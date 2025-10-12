using Domain.ValueObject;

namespace Domain.Test.ValueObject;

public class ParticipantTest
{
    [Fact]
    public void TestInitialization()
    {
        var participant = new Participant(
            nickname: new Nickname("nickname"),
            seat: new Seat(1),
            stack: new Chips(1000)
        );
        Assert.Equal(new Nickname("nickname"), participant.Nickname);
        Assert.Equal(new Seat(1), participant.Seat);
        Assert.Equal(new Chips(1000), participant.Stack);
    }

    [Fact]
    public void TestRepresentation()
    {
        var participant = new Participant(
            nickname: new Nickname("nickname"),
            seat: new Seat(1),
            stack: new Chips(1000)
        );

        Assert.Equal("nickname, #1, 1000 chip(s)", $"{participant}");
    }
}
