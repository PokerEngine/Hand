using Domain.ValueObject;

namespace Domain.Test.ValueObject;

public class TableUidTest
{
    [Fact]
    public void TestInitialization()
    {
        var value = new Guid("01234567-89ab-cdef-0123-456789abcdef");
        var tableUid = new TableUid(value);
        Assert.Equal(value, (Guid)tableUid);

        tableUid = (TableUid)value;
        Assert.Equal(value, (Guid)tableUid);
    }

    [Fact]
    public void TestComparison()
    {
        var value1 = new Guid("01234567-89ab-cdef-0123-456789abcdef");
        var value2 = new Guid("fedcba98-7654-3210-fedc-ba9876543210");

        Assert.True(new TableUid(value1) == new TableUid(value1));
        Assert.False(new TableUid(value1) == new TableUid(value2));

        Assert.False(new TableUid(value1) != new TableUid(value1));
        Assert.True(new TableUid(value1) != new TableUid(value2));
    }

    [Fact]
    public void TestRepresentation()
    {
        var value = new Guid("01234567-89ab-cdef-0123-456789abcdef");
        var tableUid = new TableUid(value);
        Assert.Equal("01234567-89ab-cdef-0123-456789abcdef", $"{tableUid}");
    }
}
