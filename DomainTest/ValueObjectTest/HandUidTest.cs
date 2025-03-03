using Domain.ValueObject;

namespace DomainTest.ValueObjectTest;

public class HandUidTest
{
    [Fact]
    public void TestInitialization()
    {
        var value = new Guid("01234567-89ab-cdef-0123-456789abcdef");
        var handUid = new HandUid(value);
        Assert.Equal(value, (Guid)handUid);

        handUid = (HandUid)value;
        Assert.Equal(value, (Guid)handUid);
    }

    [Fact]
    public void TestComparison()
    {
        var value1 = new Guid("01234567-89ab-cdef-0123-456789abcdef");
        var value2 = new Guid("fedcba98-7654-3210-fedc-ba9876543210");

        Assert.True(new HandUid(value1) == new HandUid(value1));
        Assert.False(new HandUid(value1) == new HandUid(value2));

        Assert.False(new HandUid(value1) != new HandUid(value1));
        Assert.True(new HandUid(value1) != new HandUid(value2));
    }

    [Fact]
    public void TestRepresentation()
    {
        var value = new Guid("01234567-89ab-cdef-0123-456789abcdef");
        var handUid = new HandUid(value);
        Assert.Equal("01234567-89ab-cdef-0123-456789abcdef", $"{handUid}");
    }
}