using System.Text.RegularExpressions;

namespace Application.IntegrationEvent;

public readonly struct IntegrationEventQueue : IEquatable<IntegrationEventQueue>
{
    private readonly string _name;

    private static readonly Regex Pattern = new(
        "^[a-z][a-z0-9_-]*(\\.[a-z][a-z0-9_-]*)*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public IntegrationEventQueue(string name)
    {
        if (!Pattern.IsMatch(name))
        {
            throw new ArgumentException("IntegrationEventQueue must start with a latin letter and contain only latin letters, digits, underscore symbols and dashes");
        }

        _name = name;
    }

    public static implicit operator string(IntegrationEventQueue a)
        => a._name;

    public static explicit operator IntegrationEventQueue(string a)
        => new(a);

    public bool IsSubQueue(IntegrationEventQueue other)
    {
        if (_name == other._name)
        {
            return true;
        }

        var parts = Split().ToList();
        var otherParts = other.Split().ToList();

        if (parts.Count > otherParts.Count)
        {
            return false;
        }

        for (var i = 0; i < parts.Count; i++)
        {
            if (parts[i] != otherParts[i])
            {
                return false;
            }
        }

        return true;
    }

    public IEnumerable<IntegrationEventQueue> Split()
    {
        return _name.Split(".").Select(x => new IntegrationEventQueue(x));
    }

    public bool Equals(IntegrationEventQueue other)
        => _name.Equals(other._name);
}
