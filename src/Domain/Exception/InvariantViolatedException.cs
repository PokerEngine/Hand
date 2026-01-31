namespace Domain.Exception;

public abstract class InvariantViolatedException(string message) : System.Exception(message);

public class InsufficientChipsException(string message) : InvariantViolatedException(message);

public class InsufficientCardException(string message) : InvariantViolatedException(message);

public class InvalidHandStateException(string message) : InvariantViolatedException(message);
