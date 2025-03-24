using System.Collections;
using System.Collections.Immutable;

using Domain.Error;

namespace Domain.ValueObject;

public readonly struct SidePot : IEnumerable<KeyValuePair<Nickname, Chips>>, IEquatable<SidePot>
{
    private readonly ImmutableDictionary<Nickname, Chips> _mapping;
    private readonly Chips _deadAmount;

    public Chips Amount
    {
        get
        {
            var amount = _deadAmount;

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
        _deadAmount = new Chips(0);
    }

    private SidePot(IDictionary<Nickname, Chips> mapping, Chips deadAmount)
    {
        _mapping = mapping.ToImmutableDictionary();
        _deadAmount = deadAmount;
    }

    public Chips Get(Nickname nickname)
    {
        if (!_mapping.TryGetValue(nickname, out var amount))
        {
            amount = new Chips(0);
        }
        return amount;
    }

    public Chips GetDead()
    {
        return _deadAmount;
    }

    public SidePot Add(Nickname nickname, Chips amount)
    {
        if (!amount)
        {
            throw new NotAvailableError("Cannot add zero amount");
        }
        return new SidePot(_mapping.SetItem(nickname, Get(nickname) + amount), _deadAmount);
    }

    public SidePot AddDead(Chips amount)
    {
        if (!amount)
        {
            throw new NotAvailableError("Cannot add zero amount");
        }
        return new SidePot(_mapping, _deadAmount + amount);
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
            return new SidePot(_mapping.Remove(nickname), _deadAmount);
        }
        return new SidePot(_mapping.SetItem(nickname, Get(nickname) - amount), _deadAmount);
    }

    public SidePot SubDead(Chips amount)
    {
        if (!amount)
        {
            throw new NotAvailableError("Cannot sub zero amount");
        }

        if (amount > _deadAmount)
        {
            throw new NotAvailableError("Cannot sub more amount than added");
        }

        return new SidePot(_mapping, _deadAmount - amount);
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

        return new SidePot(mapping.ToImmutableDictionary(), other._deadAmount);
    }

    public IEnumerator<KeyValuePair<Nickname, Chips>> GetEnumerator()
    {
        foreach(var pair in _mapping.OrderBy(pair => (pair.Value, pair.Key)))
        {
            yield return pair;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public static bool operator ==(SidePot a, SidePot b)
        => a._mapping == b._mapping && a._deadAmount == b._deadAmount;

    public static bool operator !=(SidePot a, SidePot b)
        => a._mapping != b._mapping || a._deadAmount != b._deadAmount;

    public bool Equals(SidePot other)
        => _mapping.Equals(other._mapping) && _deadAmount.Equals(other._deadAmount);

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

    public override int GetHashCode()
        => (_mapping.GetHashCode(), _deadAmount.GetHashCode()).GetHashCode();
}
