using Domain.ValueObject;

namespace DomainTest.ValueObjectTest;

public class ParticipantTest
{
    [Fact]
    public void TestInitialization()
    {
        var participant = new Participant(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000)
        );
        Assert.Equal(new Nickname("nickname"), participant.Nickname);
        Assert.Equal(Position.Button, participant.Position);
        Assert.Equal(new Chips(1000), participant.Stake);
    }

    [Fact]
    public void TestRepresentation()
    {
        var participant = new Participant(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000)
        );

        Assert.Equal("nickname, Button, 1000 chip(s)", $"{participant}");
    }
}