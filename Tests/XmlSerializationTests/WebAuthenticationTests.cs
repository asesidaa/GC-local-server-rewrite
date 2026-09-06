using System.Net;
using System.Net.Http.Json;
using Application;
using Application.Interfaces;
using Domain.Config;
using Domain.Entities;
using Infrastructure.Persistence;
using MainServer.Authentication;
using MainServer.Controllers.API;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shared.Dto.Api;
using Shared.Models;
using WebUI.Services;

namespace XmlSerializationTests;

public class WebAuthenticationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    public async Task DisabledOrMissingSetting_AllowsAdminAndCardOperationsWithoutCookie(bool? enabled)
    {
        using var host = new AuthTestHost(enabled);
        using var client = host.Server.CreateClient();

        using var unlock = await client.PostAsync("/api/DefaultUnlock/Avatar/1", null, TestContext.Current.CancellationToken);
        Assert.True((await ReadResult<bool>(unlock)).Data);

        using var rename = await client.PostAsJsonAsync("/api/Profiles/PlayerName",
            new ClientCardDto { CardId = 1, PlayerName = "Renamed" }, TestContext.Current.CancellationToken);
        Assert.True((await ReadResult<bool>(rename)).Data);

        using var accessCode = await client.PostAsJsonAsync("/api/Auth/SetAccessCode",
            new SetAccessCodeRequest { CardId = 2, AccessCode = "new-code" }, TestContext.Current.CancellationToken);
        Assert.True((await ReadResult<bool>(accessCode)).Data);

        using var scope = host.Server.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CardDbContext>();
        Assert.Equal("Renamed", (await db.CardMains.SingleAsync(c => c.CardId == 1, TestContext.Current.CancellationToken)).PlayerName);
        Assert.Equal("[1]", (await db.DefaultUnlockStates.SingleAsync(TestContext.Current.CancellationToken)).UnlockedBitset);
        Assert.True(await db.CardAccessCodes.AnyAsync(c => c.CardId == 2, TestContext.Current.CancellationToken));
        Assert.False(unlock.Headers.Contains("Set-Cookie"));
    }

    [Theory]
    [InlineData("/api/DefaultUnlock/Avatar/1")]
    [InlineData("/api/Profiles/UnlockAllMusic/1")]
    public async Task Enabled_AnonymousAdminOperationReturns401(string path)
    {
        using var host = new AuthTestHost(true);
        using var client = host.Server.CreateClient();

        using var response = await client.PostAsync(path, null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(response.Headers.Location);
    }

    [Fact]
    public async Task Enabled_AdminLoginAllowsManagementAndLogoutRevokesAccess()
    {
        using var host = new AuthTestHost(true);
        using var client = host.Server.CreateClient();
        using var login = await client.PostAsJsonAsync("/api/Auth/LoginAdmin",
            new AdminLoginRequest { Password = "test-password" }, TestContext.Current.CancellationToken);
        Assert.Equal("Admin", (await ReadResult<AuthStatusDto>(login)).Data?.Role);
        client.DefaultRequestHeaders.Add("Cookie", GetCookie(login));

        using var unlock = await client.PostAsync("/api/DefaultUnlock/Avatar/1", null, TestContext.Current.CancellationToken);
        Assert.True((await ReadResult<bool>(unlock)).Data);
        using var logout = await client.PostAsync("/api/Auth/Logout", null, TestContext.Current.CancellationToken);
        Assert.True((await ReadResult<bool>(logout)).Data);
        Assert.Contains(logout.Headers.GetValues("Set-Cookie"), value => value.Contains("expires="));
        client.DefaultRequestHeaders.Remove("Cookie");

        using var denied = await client.PostAsync("/api/DefaultUnlock/Avatar/2", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, denied.StatusCode);
    }

    [Fact]
    public async Task Enabled_InvalidPasswordDoesNotCreateSession()
    {
        using var host = new AuthTestHost(true);
        using var client = host.Server.CreateClient();
        using var login = await client.PostAsJsonAsync("/api/Auth/LoginAdmin",
            new AdminLoginRequest { Password = "wrong" }, TestContext.Current.CancellationToken);

        Assert.False((await login.Content.ReadFromJsonAsync<ServiceResult<AuthStatusDto>>(TestContext.Current.CancellationToken))?.Succeeded);
        Assert.False(login.Headers.Contains("Set-Cookie"));
        using var denied = await client.GetAsync("/api/Coin/1", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, denied.StatusCode);
    }

    [Fact]
    public async Task Enabled_PlayerRemainsRestrictedToOwnCardAndCannotManageUnlocks()
    {
        using var host = new AuthTestHost(true);
        using var client = host.Server.CreateClient();
        using var login = await client.PostAsJsonAsync("/api/Auth/LoginCard",
            new CardLoginRequest { CardId = 1, AccessCode = "card-code" }, TestContext.Current.CancellationToken);
        Assert.Equal("Player", (await ReadResult<AuthStatusDto>(login)).Data?.Role);
        client.DefaultRequestHeaders.Add("Cookie", GetCookie(login));

        using var ownCard = await client.GetAsync("/api/Coin/1", TestContext.Current.CancellationToken);
        Assert.Equal(1, (await ReadResult<PlayerCoinDto>(ownCard)).Data?.CardId);
        using var otherCard = await client.GetAsync("/api/Coin/2", TestContext.Current.CancellationToken);
        Assert.False((await otherCard.Content.ReadFromJsonAsync<ServiceResult<PlayerCoinDto>>(TestContext.Current.CancellationToken))?.Succeeded);
        using var adminOnly = await client.PostAsync("/api/DefaultUnlock/Avatar/1", null, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, adminOnly.StatusCode);
        Assert.Null(adminOnly.Headers.Location);
    }

    [Fact]
    public async Task Disabled_WebUiReceivesManagementAccessWithoutLogin()
    {
        using var host = new AuthTestHost(null);
        using var client = host.Server.CreateClient();
        var state = new AuthStateService(client);

        await state.CheckAuthAsync();

        Assert.False(state.AuthenticationEnabled);
        Assert.True(state.IsAdmin);
        Assert.True(state.IsAuthenticated);
        Assert.False(state.IsPlayer);
        Assert.Null(state.CardId);
    }

    [Theory]
    [InlineData("/api/Auth/LoginAdmin")]
    [InlineData("/api/Auth/LoginCard")]
    public async Task Disabled_LoginDoesNotIssueCookieOrRestrictAccess(string path)
    {
        using var host = new AuthTestHost(false);
        using var client = host.Server.CreateClient();
        using var response = await client.PostAsJsonAsync(path, new
        {
            Password = "wrong", CardId = 1, AccessCode = "wrong"
        }, TestContext.Current.CancellationToken);
        var status = Assert.IsType<AuthStatusDto>((await ReadResult<AuthStatusDto>(response)).Data);

        Assert.False(status.AuthenticationEnabled);
        Assert.Equal("Admin", status.Role);
        Assert.Null(status.CardId);
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    public async Task Disabled_ExistingPlayerCookieDoesNotRestrictManagementAccess()
    {
        using var enabledHost = new AuthTestHost(true);
        using var enabledClient = enabledHost.Server.CreateClient();
        using var login = await enabledClient.PostAsJsonAsync("/api/Auth/LoginCard",
            new CardLoginRequest { CardId = 1, AccessCode = "card-code" }, TestContext.Current.CancellationToken);
        Assert.Equal("Player", (await ReadResult<AuthStatusDto>(login)).Data?.Role);

        using var disabledHost = new AuthTestHost(false);
        using var disabledClient = disabledHost.Server.CreateClient();
        disabledClient.DefaultRequestHeaders.Add("Cookie", GetCookie(login));
        using var unlock = await disabledClient.PostAsync("/api/DefaultUnlock/Avatar/1", null,
            TestContext.Current.CancellationToken);
        Assert.True((await ReadResult<bool>(unlock)).Data);
        using var otherCard = await disabledClient.GetAsync("/api/Coin/2", TestContext.Current.CancellationToken);
        Assert.Equal(2, (await ReadResult<PlayerCoinDto>(otherCard)).Data?.CardId);
    }

    [Fact]
    public async Task Disabled_LogoutKeepsWebUiManagementAccess()
    {
        using var host = new AuthTestHost(false);
        using var client = host.Server.CreateClient();
        var state = new AuthStateService(client);
        await state.CheckAuthAsync();

        await state.LogoutAsync();

        Assert.False(state.AuthenticationEnabled);
        Assert.True(state.IsAdmin);
    }

    [Fact]
    public async Task Enabled_WebUiStartsAnonymousAndReceivesLoginStatus()
    {
        using var host = new AuthTestHost(true);
        using var client = host.Server.CreateClient();
        var state = new AuthStateService(client);

        await state.CheckAuthAsync();
        Assert.True(state.AuthenticationEnabled);
        Assert.False(state.IsAuthenticated);
        Assert.False(state.IsAdmin);

        await state.LoginAdminAsync("test-password");
        Assert.True(state.AuthenticationEnabled);
        Assert.True(state.IsAdmin);
    }

    [Theory]
    [InlineData("/game/command.php")]
    [InlineData("/api-other")]
    public async Task Disabled_DoesNotGrantIdentityOutsideWebApi(string path)
    {
        using var host = new AuthTestHost(false);
        using var scope = host.Server.Services.CreateScope();
        var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        context.Request.Path = path;

        var result = await context.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.Null(result.Principal);
    }

    private static async Task<ServiceResult<T>> ReadResult<T>(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = Assert.IsType<ServiceResult<T>>(
            await response.Content.ReadFromJsonAsync<ServiceResult<T>>(TestContext.Current.CancellationToken));
        Assert.True(result.Succeeded, result.Error?.Message);
        return result;
    }

    private static string GetCookie(HttpResponseMessage response) =>
        Assert.Single(response.Headers.GetValues("Set-Cookie")).Split(';')[0];

    private sealed class AuthTestHost : IDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        private readonly IHost host;
        private static readonly IDataProtectionProvider DataProtection = new EphemeralDataProtectionProvider();
        public TestServer Server { get; }

        public AuthTestHost(bool? enabled)
        {
            connection.Open();
            var configurationValues = new Dictionary<string, string?>
            {
                ["WebAuth:AdminPassword"] = "test-password"
            };
            if (enabled.HasValue)
                configurationValues["WebAuth:Enabled"] = enabled.Value.ToString();
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(configurationValues).Build();

            host = new HostBuilder().ConfigureWebHost(web => web.UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddSingleton(DataProtection);
                    services.Configure<WebAuthConfig>(configuration.GetSection("WebAuth"));
                    services.Configure<GameConfig>(options => options.AvatarCount = 4);
                    services.AddDbContext<CardDbContext>(options => options.UseSqlite(connection));
                    services.AddScoped<ICardDbContext>(sp => sp.GetRequiredService<CardDbContext>());
                    services.AddDbContext<MusicDbContext>(options => options.UseSqlite("Data Source=:memory:"));
                    services.AddScoped<IMusicDbContext>(sp => sp.GetRequiredService<MusicDbContext>());
                    services.AddSingleton<IPasswordHasher<CardAccessCode>, PasswordHasher<CardAccessCode>>();
                    services.AddApplication();
                    services.RemoveAll<IHostedService>();
                    services.AddWebAuthentication();
                    services.AddControllers().AddApplicationPart(typeof(AuthController).Assembly);
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints => endpoints.MapControllers());
                })).Start();
            Server = host.GetTestServer();

            using var scope = Server.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CardDbContext>();
            db.Database.EnsureCreated();
            db.CardMains.AddRange(new CardMain { CardId = 1 }, new CardMain { CardId = 2 });
            var code = new CardAccessCode { CardId = 1 };
            code.HashedCode = new PasswordHasher<CardAccessCode>().HashPassword(code, "card-code");
            db.CardAccessCodes.Add(code);
            db.SaveChanges();
        }

        public void Dispose()
        {
            host.Dispose();
            connection.Dispose();
        }
    }
}
