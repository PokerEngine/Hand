namespace Application.Authentication;

public interface ICurrentUserProvider
{
    CurrentUser GetCurrentUser();
}

public record CurrentUser
{
    public required Guid SessionUid { get; init; }
    public required Guid AccountUid { get; init; }
    public required string Nickname { get; init; }
}
