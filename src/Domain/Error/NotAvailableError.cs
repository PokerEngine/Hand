namespace Domain.Error;

public class NotAvailableError : Exception
{
    public NotAvailableError()
    {
    }

    public NotAvailableError(string message) : base(message)
    {
    }

    public NotAvailableError(string message, Exception inner) : base(message, inner)
    {
    }
}
