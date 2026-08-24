using System.Net;
using GCRelayServer.Routing;
using Serilog;

namespace GCRelayServer;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#endif
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            if (!RelayServerOptions.TryParse(args, out var options, out var error))
            {
                Log.Error("{Error}", error);
                return 2;
            }

            Log.Information(
                "UDP relay port {Port}, room expiry {RoomExpiry}, cleanup interval {CleanupInterval}",
                options!.Port,
                options.RoomExpiry,
                options.CleanupInterval);

            var timeProvider = TimeProvider.System;
            var registry = new RelayRoomRegistry(timeProvider, options.RoomExpiry);
            var router = new RelayRouter(registry);
            var server = new RelayServer(
                IPAddress.Any,
                options.Port,
                new RelayPacketProcessor(router));
            var cleanupWorker = new RelayCleanupWorker(registry, options.CleanupInterval, timeProvider);
            using var shutdown = new CancellationTokenSource();

            Console.CancelKeyPress += (_, eventArgs) =>
            {
                eventArgs.Cancel = true;
                Log.Information("Ctrl+C received; stopping relay server");
                shutdown.Cancel();
            };

            Log.Information("Relay server starting");
            if (!server.Start())
            {
                throw new InvalidOperationException("NetCoreServer failed to start the UDP relay socket.");
            }

            var cleanupTask = cleanupWorker.RunAsync(shutdown.Token);
            Log.Information("Relay server started");
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, shutdown.Token);
            }
            catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
            {
                // Normal Ctrl+C shutdown.
            }
            finally
            {
                shutdown.Cancel();
                server.Stop();
                await cleanupTask;
            }

            Log.Information("Relay server stopped");
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Relay server terminated unexpectedly");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
