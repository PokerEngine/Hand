namespace Domain.Error;

public class NotFoundError : Exception
{
    public NotFoundError()
    {
    }

    public NotFoundError(string message) : base(message)
    {
    }

    public NotFoundError(string message, Exception inner) : base(message, inner)
    {
    }
}
