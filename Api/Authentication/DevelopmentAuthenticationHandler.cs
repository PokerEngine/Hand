using Application.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace Api.Authentication;

public class DevelopmentAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var header))
            return Task.FromResult(AuthenticateResult.NoResult());

        var nickname = header.ToString().Trim();
        if (string.IsNullOrEmpty(nickname))
            return Task.FromResult(AuthenticateResult.NoResult());

        Context.Items[nameof(CurrentUser)] = new CurrentUser
        {
            AccountUid = DeriveGuid(nickname),
            SessionUid = DeriveGuid(nickname + "_session"),
            Nickname = nickname
        };

        var claims = new[] { new Claim("nickname", nickname) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }

    private static Guid DeriveGuid(string input)
        => new(MD5.HashData(Encoding.UTF8.GetBytes(input)));
}
