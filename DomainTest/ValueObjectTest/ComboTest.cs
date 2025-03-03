using Domain.ValueObject;

namespace DomainTest.ValueObjectTest;

public class ComboTest
{
    [Fact]
    public void TestInitialization()
    {
        var combo = new Combo(ComboType.Straight, 100500);
        Assert.Equal(ComboType.Straight, combo.Type);
        Assert.Equal(100500, combo.Weight);
    }

    [Fact]
    public void TestComparison()
    {
        Assert.True(new Combo(ComboType.Straight, 100500) == new Combo(ComboType.Straight, 100500));
        Assert.False(new Combo(ComboType.Straight, 100500) == new Combo(ComboType.Straight, 500100));
        Assert.True(new Combo(ComboType.Straight, 100500) == new Combo(ComboType.Flush, 100500));

        Assert.False(new Combo(ComboType.Straight, 100500) != new Combo(ComboType.Straight, 100500));
        Assert.True(new Combo(ComboType.Straight, 100500) != new Combo(ComboType.Straight, 500100));
        Assert.False(new Combo(ComboType.Straight, 100500) != new Combo(ComboType.Flush, 100500));
    }

    [Fact]
    public void TestRepresentation()
    {
        var combo = new Combo(ComboType.Straight, 100500);
        Assert.Equal("Straight [100500]", $"{combo}");
    }
}