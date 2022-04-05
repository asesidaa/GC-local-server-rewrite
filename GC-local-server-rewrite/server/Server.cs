﻿using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Files;
using EmbedIO.WebApi;
using GCLocalServerRewrite.backports;
using GCLocalServerRewrite.common;
using GCLocalServerRewrite.controllers;
using GCLocalServerRewrite.models;
using Swan.Logging;

namespace GCLocalServerRewrite.server;

public class Server
{
    public static WebServer CreateWebServer(string[] urlPrefixes)
    {
        InitializeDatabase();
        var cert = CertificateHelper.InitializeCertificate();

        var server = new WebServer(webServerOptions => webServerOptions
                .WithUrlPrefixes(urlPrefixes)
                .WithCertificate(cert)
                .WithMode(HttpListenerMode.EmbedIO))
            .WithLocalSessionManager()
            .WithCors(
                "http://unosquare.github.io,http://run.plnkr.co",
                "content-type, accept",
                "post")
            .WithWebApi(Configs.CARD_SERVICE_BASE_ROUTE, CustomResponseSerializer.None(true),
                module => module.WithController<CardServiceController>())
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

        // Listen for state changes.
        server.StateChanged += (_, e) => $"WebServer New State - {e.NewState}".Info();

        return server;
    }

    private static void InitializeDatabase()
    {
        DatabaseHelper.InitializeLocalDatabase(Configs.CARD_DB_NAME,
            typeof(Card), typeof(CardBData), typeof(CardDetail));
        DatabaseHelper.InitializeLocalDatabase(Configs.MUSIC_DB_NAME,
            typeof(Music), typeof(MusicAou), typeof(MusicExtra));
    }
}