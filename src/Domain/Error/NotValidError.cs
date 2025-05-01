namespace Domain.Error;

public class NotValidError : Exception
{
    public NotValidError()
    {
    }

    public NotValidError(string message) : base(message)
    {
    }

    public NotValidError(string message, Exception inner) : base(message, inner)
    {
    }
}
