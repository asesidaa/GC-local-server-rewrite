using System.Security.Cryptography.X509Certificates;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Files;
using EmbedIO.WebApi;
using GCLocalServerRewrite.common;
using GCLocalServerRewrite.controllers;
using GCLocalServerRewrite.models;
using Swan.Logging;
using System.Text;
using Swan;

namespace GCLocalServerRewrite.server;

public class Server
{
    public static WebServer CreateWebServer(IEnumerable<string> urlPrefixes)
    {
        InitializeDatabase();
        X509Certificate2 cert;
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            cert = CertificateHelper.InitializeCertificate();
        }
        else
        {
            var certPath = Path.Combine(PathHelper.CertRootPath, "cert.pfx");
            var certPassword = string.Empty;
            var collection = new X509Certificate2Collection();
            collection.Import(certPath, null, X509KeyStorageFlags.PersistKeySet |
                                              X509KeyStorageFlags.MachineKeySet | 
                                              X509KeyStorageFlags.Exportable);
            if (!collection.Any())
            {
                SelfCheck.Failure("Failed to import certificate!!!");
            }
            cert = collection.First();
        }

        var server = new WebServer(webServerOptions => webServerOptions
                                       .WithUrlPrefixes(urlPrefixes)
                                       .WithCertificate(cert)
                                       .WithMode(HttpListenerMode.EmbedIO))
            .WithLocalSessionManager()
            .WithCors()
            .WithWebApi(Configs.API_BASE_ROUTE, module => module.WithController<ApiController>())
            .WithWebApi(Configs.CARD_SERVICE_BASE_ROUTE, ResponseSerializer.None(true),
                module => module.WithController<CardServiceController>())
            .WithWebApi(Configs.OPTION_SERVICE_BASE_ROUTE, ResponseSerializer.None(true),
                module => module.WithController<OptionServiceController>())
            .WithWebApi(Configs.UPLOAD_SERVICE_BASE_ROUTE, ResponseSerializer.None(true),
                module => module.WithController<UploadServiceController>())
            .WithWebApi(Configs.RESPONE_SERVICE_BASE_ROUTE, ResponseSerializer.None(true),
                module => module.WithController<ResponeServiceController>())
            .WithWebApi(Configs.INCOM_SERVICE_BASE_ROUTE, ResponseSerializer.None(true),
                module => module.WithController<IncomServiceController>())
            .WithWebApi(Configs.ALIVE_BASE_ROUTE, ResponseSerializer.None(true),
                module => module.WithController<AliveController>())
            .WithWebApi(Configs.SERVER_BASE_ROUTE, ResponseSerializer.None(true),
                module => module.WithController<ServerController>())
            .WithWebApi(Configs.RANK_BASE_ROUTE, ResponseSerializer.None(true),
                module => module.WithController<RankController>())
            .WithWebApi(Configs.UPDATE_SERVICE_BASE_ROUTE, ResponseSerializer.None(true),
                module => module.WithController<UpdateController>())
            .WithStaticFolder(Configs.STATIC_BASE_ROUTE, PathHelper.HtmlRootPath, true, m => m
                                  .WithContentCaching(Configs.USE_FILE_CACHE))
            // Add static files after other modules to avoid conflicts
            .WithStaticFolder("/", PathHelper.HtmlRootPath, true, m =>
            {
                m.WithContentCaching(Configs.USE_FILE_CACHE);
                m.OnMappingFailed = async (context, info) =>
                {
                    var htmlContents = await File.ReadAllTextAsync(Path.Combine(PathHelper.HtmlRootPath, "index.html"));
                    context.Response.StatusCode = 200;
                    await context.SendStringAsync(htmlContents, "text/html", Encoding.UTF8);
                };
            })
            .WithModule(new ActionModule("/", HttpVerbs.Any,
                ctx => ctx.SendDataAsync(new { Message = "Error" })));
        server.AddCustomMimeType(".dll", "application/octet-stream");
        server.AddCustomMimeType(".blat", "application/octet-stream");
        server.AddCustomMimeType(".dat", "application/octet-stream");
        server.AddCustomMimeType(".json", "application/json");
        server.AddCustomMimeType(".wasm", "application/wasm");
        server.AddCustomMimeType(".woff", "application/font-woff");
        server.AddCustomMimeType(".woff2", "application/font-woff2");

        // Listen for state changes.
        server.StateChanged += (_, e) => $"WebServer New State - {e.NewState}".Info();

        return server;
    }

    private static void InitializeDatabase()
    {
        DatabaseHelper.InitializeLocalDatabase(Configs.SETTINGS.CardDbName,
            typeof(Card), typeof(CardBData), typeof(CardDetail), typeof(CardPlayCount));
        DatabaseHelper.InitializeLocalDatabase(Configs.SETTINGS.MusicDbName,
            typeof(Music), typeof(MusicAou), typeof(MusicExtra));
    }
}