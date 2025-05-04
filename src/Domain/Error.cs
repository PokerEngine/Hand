namespace Domain;

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

public class NotPerformedError : Exception
{
    public NotPerformedError()
    {
    }

    public NotPerformedError(string message) : base(message)
    {
    }

    public NotPerformedError(string message, Exception inner) : base(message, inner)
    {
    }
}
