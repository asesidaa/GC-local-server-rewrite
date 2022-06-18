using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Files;
using EmbedIO.WebApi;
using GCLocalServerRewrite.backports;
using GCLocalServerRewrite.common;
using GCLocalServerRewrite.controllers;
using GCLocalServerRewrite.models;
using Swan.Logging;
using System.Text;

namespace GCLocalServerRewrite.server;

public class Server
{
    public static WebServer CreateWebServer(IEnumerable<string> urlPrefixes)
    {
        InitializeDatabase();
        var cert = CertificateHelper.InitializeCertificate();

        var server = new WebServer(webServerOptions => webServerOptions
                .WithUrlPrefixes(urlPrefixes)
                .WithCertificate(cert)
                .WithMode(HttpListenerMode.EmbedIO))
            .WithLocalSessionManager()
            .WithCors()
            .WithWebApi(Configs.API_BASE_ROUTE, module => module.WithController<ApiController>())
            .WithWebApi(Configs.CARD_SERVICE_BASE_ROUTE, CustomResponseSerializer.None(true),
                module => module.WithController<CardServiceController>())
            .WithWebApi(Configs.OPTION_SERVICE_BASE_ROUTE, CustomResponseSerializer.None(true),
                module => module.WithController<OptionServiceController>())
            .WithWebApi(Configs.UPLOAD_SERVICE_BASE_ROUTE, CustomResponseSerializer.None(true),
                module => module.WithController<UploadServiceController>())
            .WithWebApi(Configs.RESPONE_SERVICE_BASE_ROUTE, CustomResponseSerializer.None(true),
                module => module.WithController<ResponeServiceController>())
            .WithWebApi(Configs.INCOM_SERVICE_BASE_ROUTE, CustomResponseSerializer.None(true),
                module => module.WithController<IncomServiceController>())
            .WithWebApi(Configs.ALIVE_BASE_ROUTE, CustomResponseSerializer.None(true),
                module => module.WithController<AliveController>())
            .WithWebApi(Configs.SERVER_BASE_ROUTE, CustomResponseSerializer.None(true),
                module => module.WithController<ServerController>())
            .WithWebApi(Configs.RANK_BASE_ROUTE, CustomResponseSerializer.None(true),
                module => module.WithController<RankController>())
            .WithStaticFolder(Configs.STATIC_BASE_ROUTE, PathHelper.HtmlRootPath, true, m => m
                .WithContentCaching(Configs.USE_FILE_CACHE))

            // Add static files after other modules to avoid conflicts
            .WithStaticFolder("/", PathHelper.HtmlRootPath, true, m => m
                .WithContentCaching(Configs.USE_FILE_CACHE))
            .WithModule(new ActionModule("/", HttpVerbs.Any,
                ctx => ctx.SendDataAsync(new { Message = "Error" })));
        server.HandleHttpException(async (context, exception) =>
        {
            context.Response.StatusCode = exception.StatusCode;

            switch (exception.StatusCode)
            {
                case 404:
                    await context.SendStringAsync("404 NOT FOUND!", "text/html", new UTF8Encoding(false));
                    break;
                default:
                    await HttpExceptionHandler.Default(context, exception);
                    break;
            }
        });

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