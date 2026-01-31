namespace Application.Exception;

public abstract class NotFoundException : System.Exception
{
    protected NotFoundException(string message) : base(message) { }

    protected NotFoundException(string message, System.Exception innerException) : base(message, innerException) { }
}

public class HandNotFoundException : NotFoundException
{
    public HandNotFoundException(string message) : base(message) { }

    public HandNotFoundException(string message, System.Exception innerException) : base(message, innerException) { }
}
