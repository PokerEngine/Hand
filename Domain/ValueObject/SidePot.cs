using System.Collections;
using System.Collections.Immutable;

using Domain.Error;

namespace Domain.ValueObject;

public readonly struct SidePot : IEnumerable<KeyValuePair<Nickname, Chips>>, IEquatable<SidePot>
{
    public readonly ImmutableDictionary<Nickname, Chips> Mapping;
    public readonly Chips DeadAmount;

    public Chips Amount
    {
        get
        {
            var amount = DeadAmount;

            foreach (var value in Mapping.Values)
            {
                amount += value;
            }

            return amount;
        }
    }

    public int Count
    {
        get => Mapping.Count;
    }

    public IEnumerable<Nickname> Nicknames
    {
        get => Mapping.Keys;
    }

    public IEnumerable<Chips> Amounts
    {
        get => Mapping.Values;
    }

    public SidePot()
    {
        Mapping = ImmutableDictionary<Nickname, Chips>.Empty;
        DeadAmount = new Chips(0);
    }

    public SidePot(IDictionary<Nickname, Chips> mapping, Chips deadAmount)
    {
        Mapping = mapping.ToImmutableDictionary();
        DeadAmount = deadAmount;
    }

    public Chips Get(Nickname nickname)
    {
        if (!Mapping.TryGetValue(nickname, out var amount))
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
        return new SidePot(Mapping.SetItem(nickname, Get(nickname) + amount), DeadAmount);
    }

    public SidePot AddDead(Chips amount)
    {
        if (!amount)
        {
            throw new NotAvailableError("Cannot add zero amount");
        }
        return new SidePot(Mapping, DeadAmount + amount);
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
            return new SidePot(Mapping.Remove(nickname), DeadAmount);
        }
        return new SidePot(Mapping.SetItem(nickname, Get(nickname) - amount), DeadAmount);
    }

    public SidePot SubDead(Chips amount)
    {
        if (!amount)
        {
            throw new NotAvailableError("Cannot sub zero amount");
        }

        if (amount > DeadAmount)
        {
            throw new NotAvailableError("Cannot sub more amount than added");
        }

        return new SidePot(Mapping, DeadAmount - amount);
    }

    public SidePot Merge(SidePot other)
    {
        var mapping = Mapping.ToDictionary();

        foreach (var (nickname, amount) in other.Mapping)
        {
            if (!mapping.TryAdd(nickname, amount))
            {
                mapping[nickname] += amount;
            }
        }

        return new SidePot(mapping.ToImmutableDictionary(), other.DeadAmount);
    }

    public IEnumerator<KeyValuePair<Nickname, Chips>> GetEnumerator()
    {
        foreach(var pair in Mapping.OrderBy(pair => (pair.Value, pair.Key)))
        {
            yield return pair;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public static bool operator ==(SidePot a, SidePot b)
        => a.Mapping == b.Mapping && a.DeadAmount == b.DeadAmount;

    public static bool operator !=(SidePot a, SidePot b)
        => a.Mapping != b.Mapping || a.DeadAmount != b.DeadAmount;

    public bool Equals(SidePot other)
        => Mapping.Equals(other.Mapping) && DeadAmount.Equals(other.DeadAmount);

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
        => Mapping.GetHashCode();
}
