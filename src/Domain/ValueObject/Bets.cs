using System.Collections;
using System.Collections.Immutable;

namespace Domain.ValueObject;

public readonly struct Bets : IEnumerable<KeyValuePair<Nickname, Chips>>
{
    private readonly ImmutableDictionary<Nickname, Chips> _mapping;

    public Bets()
    {
        _mapping = ImmutableDictionary<Nickname, Chips>.Empty;
    }

    private Bets(ImmutableDictionary<Nickname, Chips> mapping)
    {
        _mapping = mapping;
    }

    public Chips TotalAmount =>
        _mapping.Values.Aggregate(Chips.Zero, (sum, x) => sum + x);

    public static Bets operator +(Bets a, Bets b)
    {
        var result = a._mapping;

        foreach (var (nickname, amount) in b._mapping)
        {
            var existing = result.TryGetValue(nickname, out var v) ? v : Chips.Zero;
            result = result.SetItem(nickname, existing + amount);
        }

        return new Bets(result);
    }

    public static Bets operator -(Bets a, Bets b)
    {
        var result = a._mapping;

        foreach (var (nickname, amount) in b._mapping)
        {
            var existing = result.TryGetValue(nickname, out var v) ? v : Chips.Zero;
            result = result.SetItem(nickname, existing - amount);
        }

        return new Bets(result);
    }

    public Chips GetAmountPostedBy(Nickname nickname)
    {
        return _mapping.TryGetValue(nickname, out var amount)
            ? amount
            : Chips.Zero;
    }

    public Chips GetMaxAmountPostedNotBy(Nickname nickname)
    {
        var posted = _mapping
            .Where(kv => kv.Key != nickname)
            .Select(kv => kv.Value);

        return posted.Any() ? posted.Max() : Chips.Zero;
    }

    public Nickname? GetNicknamePostedMaxAmount()
    {
        if (_mapping.IsEmpty)
        {
            return null;
        }

        var maxAmount = _mapping.Values.Max();
        return _mapping.First(kv => kv.Value.Equals(maxAmount)).Key;
    }

    public Bets Post(Nickname nickname, Chips amount)
    {
        var newAmount = GetAmountPostedBy(nickname) + amount;

        var updated = _mapping.SetItem(nickname, newAmount);
        return new Bets(updated);
    }

    public Bets Refund(Nickname nickname, Chips amount)
    {
        var current = GetAmountPostedBy(nickname);

        if (amount > current)
        {
            throw new InvalidOperationException("Cannot refund more than posted");
        }

        var newAmount = current - amount;

        var updated = newAmount.IsZero
            ? _mapping.Remove(nickname)
            : _mapping.SetItem(nickname, newAmount);

        return new Bets(updated);
    }

    public IEnumerator<KeyValuePair<Nickname, Chips>> GetEnumerator()
    {
        foreach (var pair in _mapping.OrderBy(pair => (pair.Value, pair.Key)))
        {
            yield return pair;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString()
    {
        List<string> parts = new();

        foreach (var pair in this)
        {
            parts.Add($"{pair.Key}: {pair.Value}");
        }

        var str = String.Join(", ", parts);
        return $"{{{str}}}";
    }
}
