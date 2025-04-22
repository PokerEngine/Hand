using System.Collections;
using System.Collections.Immutable;

namespace Domain.ValueObject;

public readonly struct CardSet : IReadOnlySet<Card>
{
    private readonly ImmutableSortedSet<Card> _cards;

    public CardSet(IEnumerable<Card> cards)
    {
        _cards = cards.ToImmutableSortedSet();
    }

    public CardSet()
    {
        _cards = ImmutableSortedSet<Card>.Empty;
    }

    public int Count => _cards.Count;

    public bool Contains(Card item)
        => _cards.Contains(item);

    public bool IsSubsetOf(IEnumerable<Card> other)
        => _cards.IsSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<Card> other)
        => _cards.IsSupersetOf(other);

    public bool IsProperSubsetOf(IEnumerable<Card> other)
        => _cards.IsProperSubsetOf(other);

    public bool IsProperSupersetOf(IEnumerable<Card> other)
        => _cards.IsProperSupersetOf(other);

    public bool Overlaps(IEnumerable<Card> other)
        => _cards.Overlaps(other);

    public bool SetEquals(IEnumerable<Card> other)
        => _cards.SetEquals(other);

    public CardSet Add(Card item)
        => new(_cards.Add(item));

    public CardSet Remove(Card item)
        => new(_cards.Remove(item));

    public CardSet Except(CardSet other)
        => new(_cards.Except(other));

    public override bool Equals(object? o)
    {
        if (o is null || o.GetType() != GetType())
            return false;
        var x = (CardSet)o;
        return _cards == x.ToImmutableSortedSet();
    }

    public IEnumerator<Card> GetEnumerator()
    {
        foreach (var card in _cards)
        {
            yield return card;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public static CardSet operator +(CardSet a, CardSet b)
        => new(a._cards.Concat(b));

    public static bool operator ==(CardSet a, CardSet b)
        => a._cards.SetEquals(b._cards);

    public static bool operator !=(CardSet a, CardSet b)
        => !a._cards.SetEquals(b._cards);

    public override string ToString()
    {
        return $"{{{String.Join(", ", _cards)}}}";
    }

    public override int GetHashCode()
        => _cards.GetHashCode();
}
