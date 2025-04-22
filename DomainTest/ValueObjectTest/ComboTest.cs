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
    public void TestCompareTo()
    {
        Assert.Equal(0, new Combo(ComboType.Straight, 100500).CompareTo(new Combo(ComboType.Flush, 100500)));
        Assert.Equal(1, new Combo(ComboType.Straight, 100501).CompareTo(new Combo(ComboType.Flush, 100500)));
        Assert.Equal(-1, new Combo(ComboType.Straight, 100500).CompareTo(new Combo(ComboType.Flush, 100501)));
    }

    [Fact]
    public void TestRepresentation()
    {
        var combo = new Combo(ComboType.Straight, 100500);
        Assert.Equal("Straight [100500]", $"{combo}");
    }
}