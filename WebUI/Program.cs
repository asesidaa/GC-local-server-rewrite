using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WebUI;
using MudBlazor.Services;
using WebUI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTransient<CookieHandler>();
builder.Services.AddHttpClient("Default", client =>
    {
        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    })
    .AddHttpMessageHandler<CookieHandler>();

// Register a default HttpClient that uses the named client with cookie support
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("Default"));

builder.Services.AddMudServices();
builder.Services.AddSingleton<IDataService, DataService>();
builder.Services.AddSingleton<AuthStateService>();

var host = builder.Build();

var service = host.Services.GetRequiredService<IDataService>();
await service.InitializeAsync();

await host.RunAsync();
