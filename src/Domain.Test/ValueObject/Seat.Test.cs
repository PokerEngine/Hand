using Domain.ValueObject;

namespace Domain.Test.ValueObject;

public class SeatTest
{
    [Fact]
    public void TestInitialization()
    {
        var seat = new Seat(5);
        Assert.Equal(5, (int)seat);

        seat = (Seat)5;
        Assert.Equal(5, (int)seat);
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(0)]
    [InlineData(10)]
    public void TestInitializationWithInvalidNumber(int number)
    {
        Seat seat;

        var exc = Assert.Throws<ArgumentOutOfRangeException>(() => seat = new Seat(number));
        Assert.StartsWith("Seat must be between 1 and 9", exc.Message);

        exc = Assert.Throws<ArgumentOutOfRangeException>(() => seat = (Seat)(number));
        Assert.StartsWith("Seat must be between 1 and 9", exc.Message);
    }

    [Fact]
    public void TestCompareTo()
    {
        var seat = new Seat(5);

        Assert.Equal(0, seat.CompareTo(new Seat(5)));
        Assert.Equal(1, seat.CompareTo(new Seat(4)));
        Assert.Equal(-1, seat.CompareTo(new Seat(6)));
    }

    [Fact]
    public void TestEquals()
    {
        var seat = new Seat(5);

        Assert.True(seat.Equals(new Seat(5)));
        Assert.False(seat.Equals(new Seat(4)));
    }

    [Fact]
    public void TestComparison()
    {
        Assert.True(new Seat(5) == new Seat(5));
        Assert.False(new Seat(5) == new Seat(8));

        Assert.False(new Seat(5) != new Seat(5));
        Assert.True(new Seat(5) != new Seat(8));

        Assert.False(new Seat(5) > new Seat(5));
        Assert.False(new Seat(5) > new Seat(8));

        Assert.True(new Seat(5) >= new Seat(5));
        Assert.False(new Seat(5) >= new Seat(8));

        Assert.False(new Seat(5) < new Seat(5));
        Assert.True(new Seat(5) < new Seat(8));

        Assert.True(new Seat(5) <= new Seat(5));
        Assert.True(new Seat(5) <= new Seat(8));
    }

    [Fact]
    public void TestRepresentation()
    {
        var seat = new Seat(5);
        Assert.Equal("#5", $"{seat}");
    }
}
