using System.Text.RegularExpressions;

namespace Domain.ValueObject;

public readonly struct Nickname : IComparable<Nickname>, IEquatable<Nickname>
{
    private readonly string _name;

    private static readonly Regex Pattern = new (
        "^[a-z][a-z0-9_]*$", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );
    private const int MinLength = 4;
    private const int MaxLength = 32;

    public Nickname(string name)
    {
        if (name.Length < MinLength)
        {
            throw new ArgumentException($"Nickname must contain at least {MinLength} symbol(s)");
        }
        if (name.Length > MaxLength)
        {
            throw new ArgumentException($"Nickname must not contain more than {MaxLength} symbol(s)");
        }
        if (!Pattern.IsMatch(name))
        {
            throw new ArgumentException(
                "Nickname must start with a latin letter and contain only latin letters, digits and underscore symbols"
            );
        }

        _name = name;
    }

    public static implicit operator string(Nickname a)
        => a._name;

    public static explicit operator Nickname(string a)
        => new (a);

    public static bool operator ==(Nickname a, Nickname b)
        => a._name == b._name;

    public static bool operator !=(Nickname a, Nickname b)
        => a._name != b._name;

    public int CompareTo(Nickname other)
        => _name.CompareTo(other._name);

    public bool Equals(Nickname other)
        => _name.Equals(other._name);

    public override int GetHashCode()
        => _name.GetHashCode();

    public override string ToString()
        => _name;
}