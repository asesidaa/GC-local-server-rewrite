using System.Net;
using Serilog;

namespace GCRelayServer;

public static class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#endif
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            var port = ParsePort(args);

            Log.Information("UDP relay server port: {Port}", port);

            var roomManager = new RoomManager();
            var server = new RelayServer(IPAddress.Any, port, roomManager);

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                Log.Information("Ctrl+C received, shutting down...");
                server.Stop();
            };

            Log.Information("Server starting...");
            server.Start();
            Log.Information("Server started. Press Ctrl+C to stop or Enter to exit.");

            for (;;)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }

                if (line == "!")
                {
                    Log.Information("Server restarting...");
                    server.Restart();
                    Log.Information("Server restarted");
                }
            }

            Log.Information("Server stopping...");
            server.Stop();
            Log.Information("Server stopped");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Relay server terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static int ParsePort(string[] args)
    {
        const int defaultPort = 3333;

        if (args.Length == 0)
        {
            return defaultPort;
        }

        if (!int.TryParse(args[0], out var port) || port is < 1 or > 65535)
        {
            Log.Warning("Invalid port '{Arg}', using default {Default}", args[0], defaultPort);
            return defaultPort;
        }

        return port;
    }
}
