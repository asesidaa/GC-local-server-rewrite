using Domain.Config;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace MainServer.Authentication;

public static class WebAuthenticationExtensions
{
    public static IServiceCollection AddWebAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
                // Return 401/403 instead of redirecting API requests to a login page.
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            })
            .AddScheme<AuthenticationSchemeOptions, DisabledWebAuthenticationHandler>(
                DisabledWebAuthenticationHandler.SchemeName, _ => { });
        services.AddOptions<AuthenticationOptions>()
            .Configure<IOptions<WebAuthConfig>>((options, config) =>
            {
                options.DefaultAuthenticateScheme = config.Value.Enabled
                    ? CookieAuthenticationDefaults.AuthenticationScheme
                    : DisabledWebAuthenticationHandler.SchemeName;
            });
        services.AddAuthorization();
        return services;
    }
}
