using Domain.Error;
using Domain.ValueObject;

namespace Domain.Entity;

public class Player : IEquatable<Player>
{
    public Nickname Nickname { get; }
    public Position Position { get; }
    public Chips Stake { get; private set; }
    public CardSet HoleCards { get; private set; }
    public bool IsConnected { get; private set; }
    public bool IsFolded { get; private set; }

    public bool IsAllIn
    {
        get => !Stake;
    }

    public bool IsAvailableForDealing
    {
        get => !IsFolded;
    }

    public bool IsAvailableForTrading
    {
        get => !IsFolded && !IsAllIn;
    }

    public bool IsAvailableForShowdown
    {
        get => !IsFolded;
    }

    public Player(Nickname nickname, Position position, Chips stake, CardSet holeCards, bool isConnected, bool isFolded)
    {
        Nickname = nickname;
        Position = position;
        Stake = stake;
        HoleCards = holeCards;
        IsConnected = isConnected;
        IsFolded = isFolded;
    }

    public void Connect()
    {
        if (IsConnected)
        {
            throw new NotAvailableError("The player has already connected");
        }

        IsConnected = true;
    }

    public void Disconnect()
    {
        if (!IsConnected)
        {
            throw new NotAvailableError("The player has not connected yet");
        }

        IsConnected = false;
    }

    public void TakeHoleCards(CardSet holeCards)
    {
        if (IsFolded)
        {
            throw new NotAvailableError("The player has already folded");
        }

        HoleCards += holeCards;
    }

    public void Fold()
    {
        if (IsFolded)
        {
            throw new NotAvailableError("The player has already folded");
        }
        if (IsAllIn)
        {
            throw new NotAvailableError("The player has already been all in");
        }

        IsFolded = true;
    }

    public void Check()
    {
        if (!IsConnected)
        {
            throw new NotAvailableError("The player has not connected yet");
        }
        if (IsFolded)
        {
            throw new NotAvailableError("The player has already folded");
        }
        if (IsAllIn)
        {
            throw new NotAvailableError("The player has already been all in");
        }
    }

    public void Post(Chips amount)
    {
        if (!IsConnected)
        {
            throw new NotAvailableError("The player has not connected yet");
        }
        if (IsFolded)
        {
            throw new NotAvailableError("The player has already folded");
        }
        if (IsAllIn)
        {
            throw new NotAvailableError("The player has already been all in");
        }
        if (Stake < amount)
        {
            throw new NotAvailableError("The player cannot post more amount than his stake");
        }

        Stake -= amount;
    }

    public void Win(Chips amount)
    {
        if (IsFolded)
        {
            throw new NotAvailableError("The player has already folded");
        }

        Stake += amount;
    }

    public void Refund(Chips amount)
    {
        if (IsFolded)
        {
            throw new NotAvailableError("The player has already folded");
        }

        Stake += amount;
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
        => $"{Nickname}, {Position}, {Stake}, {HoleCards}";
}