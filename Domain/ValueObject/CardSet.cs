using System.Collections;
using System.Collections.Immutable;

namespace Domain.ValueObject;

public readonly struct CardSet : IReadOnlySet<Card>, IReadOnlyList<Card>
{
    private readonly ImmutableList<Card> _list;
    private readonly ImmutableHashSet<Card> _set;

    public CardSet(IEnumerable<Card> cards)
    {
        _list = cards.ToImmutableList();
        _set = _list.ToImmutableHashSet();

        if (_list.Count != _set.Count)
        {
            throw new ArgumentException("CardSet cannot contain duplicates");
        }
    }

    public CardSet()
    {
        _list = ImmutableList<Card>.Empty;
        _set = _list.ToImmutableHashSet();
    }

    public int Count
    {
        get  => _set.Count;
    }

    public Card this[int index]
    {
        get => _list[index];
    }

    public CardSet Slice(int start, int length) {
        var slice = new Card[length];
        for (var i = 0; i < length; i++)
        {
            slice[i] = _list[start + i];
        }
        return new(slice);
    }

    public bool Contains(Card item)
        => _set.Contains(item);

    public bool IsSubsetOf(IEnumerable<Card> other)
        => _set.IsSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<Card> other)
        => _set.IsSupersetOf(other);

    public bool IsProperSubsetOf(IEnumerable<Card> other)
        => _set.IsProperSubsetOf(other);

    public bool IsProperSupersetOf(IEnumerable<Card> other)
        => _set.IsProperSupersetOf(other);

    public bool Overlaps(IEnumerable<Card> other)
        => _set.Overlaps(other);

    public bool SetEquals(IEnumerable<Card> other)
        => _set.SetEquals(other);

    public override bool Equals(object? o)
    {
        if (o is null || o.GetType() != GetType())
            return false;
        var x = (CardSet)o;
        return _set == x.ToImmutableHashSet();
    }

    public IEnumerator<Card> GetEnumerator()
    {
        foreach (var item in _list)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public static CardSet operator +(CardSet a, CardSet b)
        => new (a.ToImmutableList().Concat(b));

    public static bool operator ==(CardSet a, CardSet b)
        => a.ToImmutableHashSet().SetEquals(b.ToImmutableHashSet());

    public static bool operator !=(CardSet a, CardSet b)
        => !a.ToImmutableHashSet().SetEquals(b.ToImmutableHashSet());

    public override string ToString()
    {
        return $"{{{String.Join(", ", _list)}}}";
    }

    public override int GetHashCode()
        => _set.GetHashCode();
}