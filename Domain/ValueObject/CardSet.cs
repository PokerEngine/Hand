using Domain.Exception;
using System.Collections;
using System.Collections.Immutable;

namespace Domain.ValueObject;

public readonly struct CardSet : IReadOnlySet<Card>, IEquatable<CardSet>
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

    private static CardSet FromString(string value)
    {
        if (value.Length % 2 != 0)
        {
            throw new InsufficientCardException($"Unknown cards: {value}");
        }

        var cards = new List<Card>();
        for (var i = 0; i < value.Length; i += 2)
        {
            var card = (Card)value.Substring(i, 2);
            cards.Add(card);
        }

        return new CardSet(cards);
    }

    public int Count => _cards.Count;

    public static implicit operator string(CardSet a)
        => String.Join("", a._cards);

    public static implicit operator CardSet(string a)
        => FromString(a);

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

    public bool Equals(CardSet other)
        => _cards.SequenceEqual(other._cards);

    public override bool Equals(object? o)
    {
        if (o is null || o.GetType() != GetType())
            return false;
        var x = (CardSet)o;
        return _cards.SequenceEqual(x);
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
        return String.Join("", _cards);
    }

    public override int GetHashCode()
        => _cards.GetHashCode();
}
