namespace Domain.Exception;

public abstract class DomainException(string message) : System.Exception(message);

public class InvalidHandConfigurationException(string message) : DomainException(message);

public class PlayerActionNotAllowedException(string message) : DomainException(message);

public class PlayerActionNotValidException(string message) : DomainException(message);

public class PlayerNotFoundException(string message) : DomainException(message);
