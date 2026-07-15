using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace WebUI.Services;

public class CookieHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.SameOrigin);
        return base.SendAsync(request, cancellationToken);
    }
}
