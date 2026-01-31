using Domain.Exception;
using Domain.ValueObject;

namespace Domain.Entity;

public class Player
{
    public Nickname Nickname { get; }
    public Seat Seat { get; }
    public Chips Stack { get; private set; }
    public CardSet HoleCards { get; private set; }
    public bool IsFolded { get; private set; }

    public bool IsAllIn => Stack.IsZero;

    public Player(Nickname nickname, Seat seat, Chips stack)
    {
        Nickname = nickname;
        Seat = seat;
        Stack = stack;
        HoleCards = new CardSet();
        IsFolded = false;
    }

    public void TakeHoleCards(CardSet holeCards)
    {
        if (IsFolded)
        {
            throw new InvalidHandStateException("The player has already folded");
        }

        HoleCards += holeCards;
    }

    public void Fold()
    {
        if (IsFolded)
        {
            throw new InvalidHandStateException("The player has already folded");
        }
        if (IsAllIn)
        {
            throw new InvalidHandStateException("The player has already been all in");
        }

        IsFolded = true;
    }

    public void Post(Chips amount)
    {
        if (IsFolded)
        {
            throw new InvalidHandStateException("The player has already folded");
        }
        if (IsAllIn)
        {
            throw new InvalidHandStateException("The player has already been all in");
        }
        if (Stack < amount)
        {
            throw new InvalidHandStateException("The player cannot post more amount than his stack");
        }

        Stack -= amount;
    }

    public void Refund(Chips amount)
    {
        if (IsFolded)
        {
            throw new InvalidHandStateException("The player has already folded");
        }

        Stack += amount;
    }

    public PlayerState GetState()
    {
        return new PlayerState
        {
            Nickname = Nickname,
            Seat = Seat,
            Stack = Stack,
            HoleCards = HoleCards,
            IsFolded = IsFolded
        };
    }

    public override string ToString()
        => $"{GetType().Name}: {Nickname}, {Seat}, {Stack}, {HoleCards}";
}
