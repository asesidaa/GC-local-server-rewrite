using System.Reflection;
using Application;
using Application.Interfaces;
using Domain.Config;
using Infrastructure;
using Infrastructure.Common;
using Infrastructure.Persistence;
using MainServer.Filters;
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
    var builder = WebApplication.CreateBuilder(args);
    
    // Add services to the container.
    const string configurationsDirectory = "Configurations";
    builder.Configuration
        .AddJsonFile($"{configurationsDirectory}/database.json", optional: false, reloadOnChange: false)
        .AddJsonFile($"{configurationsDirectory}/game.json", optional: false, reloadOnChange: false)
        .AddJsonFile($"{configurationsDirectory}/logging.json", optional: false, reloadOnChange: false)
        .AddJsonFile($"{configurationsDirectory}/events.json", optional: true, reloadOnChange: false)
        .AddJsonFile($"{configurationsDirectory}/matching.json", optional: true, reloadOnChange: false)
        .AddJsonFile($"{configurationsDirectory}/server.json", optional: true, reloadOnChange: false);
    
    builder.Services.Configure<EventConfig>(
        builder.Configuration.GetSection(EventConfig.EVENT_SECTION));
    builder.Services.Configure<RelayConfig>(
        builder.Configuration.GetSection(RelayConfig.RELAY_SECTION));
    builder.Services.Configure<GameConfig>(
        builder.Configuration.GetSection(GameConfig.GAME_SECTION));     

    var serverIp = builder.Configuration["ServerIp"] ?? "127.0.0.1";
    var certificateManager = new CertificateService(serverIp, new SerilogLoggerFactory(Log.Logger).CreateLogger(""));
    builder.WebHost.ConfigureKestrel(options => 
        options.ConfigureHttpsDefaults(adapterOptions => 
            adapterOptions.ServerCertificate = certificateManager.InitializeCertificate()
        ));

    builder.Host.UseSerilog((context, configuration) =>
    {
        configuration.WriteTo.Console().ReadFrom.Configuration(context.Configuration);
    });

    builder.Services.AddControllers(options =>
        options.Filters.Add<ApiExceptionFilterService>());
    
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    var app = builder.Build();
    
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
    }

    // app.UseExceptionHandler();
    app.UseStaticFiles();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception in startup");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}