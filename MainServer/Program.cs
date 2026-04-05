using System.Net.Security;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using Application;
using Application.Interfaces;
using Domain.Config;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Common;
using Infrastructure.Persistence;
using MainServer.Filters;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Extensions.Logging;
using Throw;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
    .InformationalVersion;
Log.Information("GCLocalServer version {Version}", version);

Log.Information("Server starting up...");

try
{ 
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    var builder = WebApplication.CreateBuilder(args);

    // Enable static web assets resolution when running the exe directly (not via dotnet run).
    // The manifest in bin/ contains absolute paths to WebUI's wwwroot, so this works from any CWD.
    // In published output the manifest is absent, making this a no-op.
    builder.WebHost.UseStaticWebAssets();

    // Add services to the container.
    const string configurationsDirectory = "Configurations";
    builder.Configuration
        .AddJsonFile($"{configurationsDirectory}/database.json", optional: false, reloadOnChange: false)
        .AddJsonFile($"{configurationsDirectory}/game.json", optional: false, reloadOnChange: false)
        .AddJsonFile($"{configurationsDirectory}/logging.json", optional: false, reloadOnChange: false)
        .AddJsonFile($"{configurationsDirectory}/events.json", optional: true, reloadOnChange: false)
        .AddJsonFile($"{configurationsDirectory}/matching.json", optional: true, reloadOnChange: false)
        .AddJsonFile($"{configurationsDirectory}/auth.json", optional: true, reloadOnChange: false)
        .AddJsonFile($"{configurationsDirectory}/rank.json", optional: true, reloadOnChange: false)
        .AddJsonFile($"{configurationsDirectory}/server.json", optional: true, reloadOnChange: false)
        .AddJsonFile($"{configurationsDirectory}/unlock.json", optional: true, reloadOnChange: false)
        .AddJsonFile($"{configurationsDirectory}/webauth.json", optional: true, reloadOnChange: false);
    
    builder.Services.Configure<EventConfig>(
        builder.Configuration.GetSection(EventConfig.EVENT_SECTION));
    builder.Services.Configure<RelayConfig>(
        builder.Configuration.GetSection(RelayConfig.RELAY_SECTION));
    builder.Services.Configure<GameConfig>(
        builder.Configuration.GetSection(GameConfig.GAME_SECTION));
    builder.Services.Configure<AuthConfig>(
        builder.Configuration.GetSection(AuthConfig.AUTH_SECTION));
    builder.Services.Configure<UnlockConfig>(
        builder.Configuration.GetSection(UnlockConfig.UNLOCK_SECTION));
    builder.Services.Configure<WebAuthConfig>(
        builder.Configuration.GetSection(WebAuthConfig.WEB_AUTH_SECTION));
    builder.Services.AddOptions<RankConfig>()
        .Bind(builder.Configuration.GetSection(RankConfig.RANK_SECTION))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    var serverIp = builder.Configuration["ServerIp"] ?? "127.0.0.1";
    var certificateManager = new CertificateService(serverIp, new SerilogLoggerFactory(Log.Logger).CreateLogger(""));
    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
    {
            builder.WebHost.UseKestrel(options =>
                options.ConfigureHttpsDefaults(adapterOptions =>
                {
                    adapterOptions.ServerCertificate = certificateManager.InitializeCertificate();
                }));
    }


    builder.Host.UseSerilog((context, configuration) =>
    {
        configuration.WriteTo.Console().ReadFrom.Configuration(context.Configuration);
    });

    builder.Services.AddControllers(options =>
        options.Filters.Add<ApiExceptionFilterService>())
        .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);
    
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddSingleton<IPasswordHasher<CardAccessCode>, PasswordHasher<CardAccessCode>>();

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;
            // Return 401/403 JSON instead of redirecting to a login page
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
        });
    builder.Services.AddAuthorization();

    var refreshIntervalHours = builder.Configuration.GetSection(RankConfig.RANK_SECTION).
        GetValue<int>("RefreshIntervalHours");
    refreshIntervalHours.Throw().IfLessThanOrEqualTo(0);
    Log.Information("Rank refresh interval: {RefreshIntervalHours} hours", refreshIntervalHours);
    builder.Services.AddApplication(refreshIntervalHours);
    builder.Services.AddInfrastructure(builder.Configuration);
    
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
    });

    var app = builder.Build();
    
    app.UseResponseCompression();
    
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<CardDbContext>();
        db.Database.Migrate();
    }
    
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms, " +
                                  "request host: {RequestHost}";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        };
    });

    var eventService = app.Services.GetService<IEventManagerService>();
    eventService.ThrowIfNull();
    eventService.InitializeEvents();
    
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseWebAssemblyDebugging();
    }

    // Add content type for .cmp and .evt files as static files with unknown file extensions return 404 by default
    // See https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-7.0#fileextensioncontenttypeprovider
    // ReSharper disable once UseObjectOrCollectionInitializer
    var contentTypeProvider = new FileExtensionContentTypeProvider();
    contentTypeProvider.Mappings[".cmp"] = "text/plain";
    contentTypeProvider.Mappings[".evt"] = "text/plain";

    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles(new StaticFileOptions
    {
        ContentTypeProvider = contentTypeProvider
    });

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapFallbackToFile("index.html");

    app.Run();
}
catch (Exception ex) when (ex.GetType().Name is not "HostAbortedException")
{
    Log.Fatal(ex, "Unhandled exception in startup");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}