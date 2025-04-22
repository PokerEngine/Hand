using Domain.ValueObject;

namespace DomainTest.ValueObjectTest;

public class NicknameTest
{
    [Theory]
    [InlineData("alpha")]
    [InlineData("Alpha_123")]
    [InlineData("ALPHA1")]
    public void TestInitialization(string name)
    {
        var nickname = new Nickname(name);
        Assert.Equal(name, (string)nickname);

        nickname = (Nickname)name;
        Assert.Equal(name, (string)nickname);
    }

    [Fact]
    public void TestInitializationWithTooShortValue()
    {
        var name = "alp";
        Nickname nickname;

        var exc = Assert.Throws<ArgumentException>(() => nickname = new Nickname(name));
        Assert.Equal(
            "Nickname must contain at least 4 symbol(s)",
            exc.Message
        );

        exc = Assert.Throws<ArgumentException>(() => nickname = (Nickname)name);
        Assert.Equal(
            "Nickname must contain at least 4 symbol(s)",
            exc.Message
        );
    }

    [Fact]
    public void TestInitializationWithTooLongValue()
    {
        var name = "alpha_beta_gamma_delta_epsilon_zeta";
        Nickname nickname;

        var exc = Assert.Throws<ArgumentException>(() => nickname = new Nickname(name));
        Assert.Equal(
            "Nickname must not contain more than 32 symbol(s)",
            exc.Message
        );

        exc = Assert.Throws<ArgumentException>(() => nickname = (Nickname)name);
        Assert.Equal(
            "Nickname must not contain more than 32 symbol(s)",
            exc.Message
        );
    }

    [Theory]
    [InlineData("1alpha")]
    [InlineData("@lpha")]
    [InlineData("alph@")]
    [InlineData("alpha beta")]
    [InlineData("alpha!")]
    public void TestInitializationWithWrongSymbols(string name)
    {
        Nickname nickname;

        var exc = Assert.Throws<ArgumentException>(() => nickname = new Nickname(name));
        Assert.Equal(
            "Nickname must start with a latin letter and contain only latin letters, digits and underscore symbols",
            exc.Message
        );

        exc = Assert.Throws<ArgumentException>(() => nickname = (Nickname)name);
        Assert.Equal(
            "Nickname must start with a latin letter and contain only latin letters, digits and underscore symbols",
            exc.Message
        );
    }

    [Fact]
    public void TestComparison()
    {
        Assert.True(new Nickname("alpha") == new Nickname("alpha"));
        Assert.False(new Nickname("alpha") == new Nickname("beta"));

        Assert.False(new Nickname("alpha") != new Nickname("alpha"));
        Assert.True(new Nickname("alpha") != new Nickname("beta"));
    }

    [Fact]
    public void TestRepresentation()
    {
        var nickname = new Nickname("alpha");
        Assert.Equal("alpha", $"{nickname}");
    }
}
