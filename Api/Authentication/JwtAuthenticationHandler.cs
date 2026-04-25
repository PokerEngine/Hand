using Application.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Api.Authentication;

public class JwtAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<JwtAuthenticationOptions> jwtOptions
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Bearer";

    private readonly JsonWebTokenHandler _tokenHandler = new();
    private readonly TokenValidationParameters _validationParameters = new()
    {
        ValidateIssuer = true,
        ValidIssuer = jwtOptions.Value.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtOptions.Value.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Secret)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var header))
            return AuthenticateResult.NoResult();

        var value = header.ToString();
        if (!value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var token = value["Bearer ".Length..].Trim();
        var result = await _tokenHandler.ValidateTokenAsync(token, _validationParameters);

        if (!result.IsValid)
            return AuthenticateResult.Fail(result.Exception?.Message ?? "Invalid token");

        var jwt = (JsonWebToken)result.SecurityToken;

        if (!jwt.TryGetClaim("typ", out var typClaim) || typClaim.Value != "access")
            return AuthenticateResult.Fail("Invalid token type");

        if (!jwt.TryGetClaim("sid", out var sidClaim) || !Guid.TryParse(sidClaim.Value, out var sessionUid))
            return AuthenticateResult.Fail("Invalid sid claim");

        if (!Guid.TryParse(jwt.Subject, out var accountUid))
            return AuthenticateResult.Fail("Invalid sub claim");

        if (!jwt.TryGetClaim("nickname", out var nicknameClaim))
            return AuthenticateResult.Fail("Missing nickname claim");

        Context.Items[nameof(CurrentUser)] = new CurrentUser
        {
            SessionUid = sessionUid,
            AccountUid = accountUid,
            Nickname = nicknameClaim.Value
        };

        var claims = new[] { new Claim("nickname", nicknameClaim.Value) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}

public class JwtAuthenticationOptions
{
    public const string SectionName = "JwtAuthenticationHandler";
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string Secret { get; init; }
}
