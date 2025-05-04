using System.Collections;
using System.Collections.Immutable;

namespace Domain.ValueObject;

public readonly struct SidePot : IEnumerable<KeyValuePair<Nickname, Chips>>, IEquatable<SidePot>
{
    private readonly ImmutableDictionary<Nickname, Chips> _mapping;

    public Chips Amount
    {
        get
        {
            var amount = new Chips(0);

            foreach (var value in _mapping.Values)
            {
                amount += value;
            }

            return amount;
        }
    }

    public int Count
    {
        get => _mapping.Count;
    }

    public IEnumerable<Nickname> Nicknames
    {
        get => _mapping.Keys;
    }

    public IEnumerable<Chips> Amounts
    {
        get => _mapping.Values;
    }

    public SidePot()
    {
        _mapping = ImmutableDictionary<Nickname, Chips>.Empty;
    }

    public SidePot(IEnumerable<KeyValuePair<Nickname, Chips>> mapping)
    {
        _mapping = mapping.ToImmutableDictionary();
    }

    public Chips Get(Nickname nickname)
    {
        if (!_mapping.TryGetValue(nickname, out var amount))
        {
            amount = new Chips(0);
        }
        return amount;
    }

    public SidePot Add(Nickname nickname, Chips amount)
    {
        if (!amount)
        {
            throw new NotAvailableError("Cannot add zero amount");
        }
        return new SidePot(_mapping.SetItem(nickname, Get(nickname) + amount));
    }

    public SidePot Sub(Nickname nickname, Chips amount)
    {
        if (!amount)
        {
            throw new NotAvailableError("Cannot sub zero amount");
        }

        if (amount > Get(nickname))
        {
            throw new NotAvailableError("Cannot sub more amount than added");
        }

        if (amount == Get(nickname))
        {
            return new SidePot(_mapping.Remove(nickname));
        }
        return new SidePot(_mapping.SetItem(nickname, Get(nickname) - amount));
    }

    public SidePot Merge(SidePot other)
    {
        var mapping = _mapping.ToDictionary();

        foreach (var (nickname, amount) in other._mapping)
        {
            if (!mapping.TryAdd(nickname, amount))
            {
                mapping[nickname] += amount;
            }
        }

        return new SidePot(mapping.ToImmutableDictionary());
    }

    public IEnumerator<KeyValuePair<Nickname, Chips>> GetEnumerator()
    {
        foreach (var pair in _mapping.OrderBy(pair => (pair.Value, pair.Key)))
        {
            yield return pair;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public bool Equals(SidePot other)
        => _mapping.Count == other._mapping.Count && !_mapping.Except(other._mapping).Any();

    public override string ToString()
    {
        var items = new List<string>();

        foreach (var (nickname, amount) in this)
        {
            var item = $"{nickname}: {amount}";
            items.Add(item);
        }

        return $"{Amount}, {{{String.Join(", ", items)}}}";
    }
}
