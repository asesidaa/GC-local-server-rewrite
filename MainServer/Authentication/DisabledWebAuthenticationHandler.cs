using System.Security.Claims;
using System.Text.Encodings.Web;
using Domain.Config;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace MainServer.Authentication;

public class DisabledWebAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<WebAuthConfig> config) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "DisabledWebAuth";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (config.Value.Enabled || !Request.Path.StartsWithSegments("/api"))
            return Task.FromResult(AuthenticateResult.NoResult());

        // Grant access for this WebUI API request only. Never persist this identity in a cookie.
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, "Local"), new Claim(ClaimTypes.Role, "Admin")], SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
