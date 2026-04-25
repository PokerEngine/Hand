using Application.Authentication;
using System.Security.Authentication;

namespace Api.Authentication;

public class HttpContextCurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    public CurrentUser GetCurrentUser()
    {
        return httpContextAccessor.HttpContext?.Items[nameof(CurrentUser)] as CurrentUser
            ?? throw new AuthenticationException("No authentication context");
    }
}
