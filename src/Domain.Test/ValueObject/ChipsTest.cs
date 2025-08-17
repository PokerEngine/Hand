using Domain.ValueObject;

namespace Domain.Test.ValueObject;

public class ChipsTest
{
    [Fact]
    public void TestInitialization()
    {
        var chips = new Chips(5);
        Assert.Equal(5, (int)chips);

        chips = (Chips)5;
        Assert.Equal(5, (int)chips);

        chips = new Chips(0);
        Assert.Equal(0, (int)chips);

        chips = (Chips)0;
        Assert.Equal(0, (int)chips);
    }

    [Fact]
    public void TestInitializationWithNegativeAmount()
    {
        Chips chips;

        var exc = Assert.Throws<ArgumentOutOfRangeException>(() => chips = new Chips(-5));
        Assert.StartsWith("Chips amount must be a non-negative integer", exc.Message);

        exc = Assert.Throws<ArgumentOutOfRangeException>(() => chips = (Chips)(-5));
        Assert.StartsWith("Chips amount must be a non-negative integer", exc.Message);
    }

    [Fact]
    public void TestMinValue()
    {
        Assert.Equal(new Chips(0), Chips.MinValue);
    }

    [Fact]
    public void TestMaxValue()
    {
        Assert.Equal(new Chips(int.MaxValue), Chips.MaxValue);
    }

    [Fact]
    public void TestCompareTo()
    {
        var chips = new Chips(5);

        Assert.Equal(0, chips.CompareTo(new Chips(5)));
        Assert.Equal(1, chips.CompareTo(new Chips(4)));
        Assert.Equal(-1, chips.CompareTo(new Chips(6)));
    }

    [Fact]
    public void TestComparison()
    {
        Assert.True(new Chips(5) == new Chips(5));
        Assert.False(new Chips(5) == new Chips(8));

        Assert.False(new Chips(5) != new Chips(5));
        Assert.True(new Chips(5) != new Chips(8));

        Assert.False(new Chips(5) > new Chips(5));
        Assert.False(new Chips(5) > new Chips(8));

        Assert.True(new Chips(5) >= new Chips(5));
        Assert.False(new Chips(5) >= new Chips(8));

        Assert.False(new Chips(5) < new Chips(5));
        Assert.True(new Chips(5) < new Chips(8));

        Assert.True(new Chips(5) <= new Chips(5));
        Assert.True(new Chips(5) <= new Chips(8));
    }

    [Fact]
    public void TestUnaryAddition()
    {
        var chips = +new Chips(5);
        Assert.Equal(5, (int)chips);
    }

    [Fact]
    public void TestUnarySubtraction()
    {
        Chips chips;
        var exc = Assert.Throws<ArgumentOutOfRangeException>(() => chips = -new Chips(5));
        Assert.StartsWith("Chips amount must be a non-negative integer", exc.Message);
    }

    [Fact]
    public void TestAddition()
    {
        var chips = new Chips(5) + new Chips(3);
        Assert.Equal(8, (int)chips);

        chips = new Chips(5) + new Chips(0);
        Assert.Equal(5, (int)chips);
    }

    [Fact]
    public void TestSubtraction()
    {
        var chips = new Chips(5) - new Chips(3);
        Assert.Equal(2, (int)chips);

        chips = new Chips(5) - new Chips(5);
        Assert.Equal(0, (int)chips);
    }

    [Fact]
    public void TestSubtractionWithNegativeResult()
    {
        Chips chips;
        var exc = Assert.Throws<ArgumentOutOfRangeException>(() => chips = new Chips(5) - new Chips(8));
        Assert.StartsWith("Chips amount must be a non-negative integer", exc.Message);
    }

    [Fact]
    public void TestMultiplication()
    {
        var chips = new Chips(5) * 3;
        Assert.Equal(15, (int)chips);

        chips = new Chips(5) * 1;
        Assert.Equal(5, (int)chips);

        chips = new Chips(5) * 0;
        Assert.Equal(0, (int)chips);
    }

    [Fact]
    public void TestMultiplicationByNegativeNumber()
    {
        Chips chips;
        var exc = Assert.Throws<ArgumentOutOfRangeException>(() => chips = new Chips(5) * -3);
        Assert.StartsWith("Chips amount must be a non-negative integer", exc.Message);
    }

    [Fact]
    public void TestDivision()
    {
        var chips = new Chips(8) / 2;
        Assert.Equal(4, (int)chips);

        chips = new Chips(8) / 3;
        Assert.Equal(2, (int)chips);

        chips = new Chips(8) / 1;
        Assert.Equal(8, (int)chips);

        chips = new Chips(8) / 9;
        Assert.Equal(0, (int)chips);
    }

    [Fact]
    public void TestDivisionByZero()
    {
        Chips chips;
        var exc = Assert.Throws<DivideByZeroException>(() => chips = new Chips(8) / 0);
        Assert.Equal("Attempted to divide by zero.", exc.Message);
    }

    [Fact]
    public void TestDivisionByNegativeNumber()
    {
        Chips chips;
        var exc = Assert.Throws<ArgumentOutOfRangeException>(() => chips = new Chips(8) / -2);
        Assert.StartsWith("Chips amount must be a non-negative integer", exc.Message);
    }

    [Fact]
    public void TestRemainderOfDivision()
    {
        var chips = new Chips(8) % 2;
        Assert.Equal(0, (int)chips);

        chips = new Chips(8) % 3;
        Assert.Equal(2, (int)chips);

        chips = new Chips(8) % 1;
        Assert.Equal(0, (int)chips);

        chips = new Chips(8) % 9;
        Assert.Equal(8, (int)chips);
    }

    [Fact]
    public void TestRemainderOfDivisionByZero()
    {
        Chips chips;
        var exc = Assert.Throws<DivideByZeroException>(() => chips = new Chips(8) % 0);
        Assert.Equal("Attempted to divide by zero.", exc.Message);
    }

    [Fact]
    public void TestRepresentation()
    {
        var chips = new Chips(5);
        Assert.Equal("5 chip(s)", $"{chips}");
    }
}
