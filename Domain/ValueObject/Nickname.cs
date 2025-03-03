using System.Text.RegularExpressions;

namespace Domain.ValueObject;

public readonly struct Nickname : IComparable<Nickname>, IEquatable<Nickname>
{
    private string Name { get; }

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
                "Nickname must start with a latin letter and contain only latin letters, numbers and underscore symbols"
            );
        }

        Name = name;
    }

    public static implicit operator string(Nickname a)
        => a.Name;

    public static explicit operator Nickname(string a)
        => new (a);

    public static bool operator ==(Nickname a, Nickname b)
        => a.Name == b.Name;

    public static bool operator !=(Nickname a, Nickname b)
        => a.Name != b.Name;

    public int CompareTo(Nickname other)
        => Name.CompareTo(other.Name);

    public bool Equals(Nickname other)
        => Name.Equals(other.Name);

    public override int GetHashCode()
        => Name.GetHashCode();

    public override string ToString()
        => Name;
}