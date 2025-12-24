using Domain.ValueObject;

namespace Domain.Entity;

public class Player : IEquatable<Player>
{
    public Nickname Nickname { get; }
    public Seat Seat { get; }
    public Chips Stack { get; private set; }
    public CardSet HoleCards { get; private set; }
    public bool IsDisconnected { get; private set; }
    public bool IsFolded { get; private set; }

    public bool IsAllIn
    {
        get => !Stack;
    }

    public Player(Nickname nickname, Seat seat, Chips stack)
    {
        Nickname = nickname;
        Seat = seat;
        Stack = stack;
        HoleCards = new CardSet();
        IsDisconnected = false;
        IsFolded = false;
    }

    public void TakeHoleCards(CardSet holeCards)
    {
        if (IsFolded)
        {
            throw new InvalidOperationException("The player has already folded");
        }

        HoleCards += holeCards;
    }

    public void Fold()
    {
        if (IsDisconnected)
        {
            throw new InvalidOperationException("The player is disconnected");
        }
        if (IsFolded)
        {
            throw new InvalidOperationException("The player has already folded");
        }
        if (IsAllIn)
        {
            throw new InvalidOperationException("The player has already been all in");
        }

        IsFolded = true;
    }

    public void Check()
    {
        if (IsDisconnected)
        {
            throw new InvalidOperationException("The player is disconnected");
        }
        if (IsFolded)
        {
            throw new InvalidOperationException("The player has already folded");
        }
        if (IsAllIn)
        {
            throw new InvalidOperationException("The player has already been all in");
        }
    }

    public void Bet(Chips amount)
    {
        // Bet means that the player puts chips into the pot voluntarily
        if (IsDisconnected)
        {
            throw new InvalidOperationException("The player is disconnected");
        }
        if (IsFolded)
        {
            throw new InvalidOperationException("The player has already folded");
        }
        if (IsAllIn)
        {
            throw new InvalidOperationException("The player has already been all in");
        }
        if (Stack < amount)
        {
            throw new InvalidOperationException("The player cannot bet more amount than his stack");
        }

        Stack -= amount;
    }

    public void Post(Chips amount)
    {
        // Post means that the player puts chips into the pot forcibly (blinds, ante)
        if (IsFolded)
        {
            throw new InvalidOperationException("The player has already folded");
        }
        if (IsAllIn)
        {
            throw new InvalidOperationException("The player has already been all in");
        }
        if (Stack < amount)
        {
            throw new InvalidOperationException("The player cannot post more amount than his stack");
        }

        Stack -= amount;
    }

    public void Win(Chips amount)
    {
        if (IsFolded)
        {
            throw new InvalidOperationException("The player has already folded");
        }

        Stack += amount;
    }

    public void Refund(Chips amount)
    {
        if (IsFolded)
        {
            throw new InvalidOperationException("The player has already folded");
        }

        Stack += amount;
    }

    public bool Equals(Player? other)
    {
        return Nickname == other?.Nickname;
    }

    public override int GetHashCode()
    {
        return Nickname.GetHashCode();
    }

    public override string ToString()
        => $"{Nickname}, {Seat}, {Stack}, {HoleCards}";
}
