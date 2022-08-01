using System.Net;
using Swan.Logging;

namespace GCRelayServer
{

    public static class Program
    {
        public static void Main(string[] args)
        {
#if DEBUG
            ConsoleLogger.Instance.LogLevel = LogLevel.Debug;
#endif
            // UDP server port
            var port = 3333;
            if (args.Length > 0)
            {
                port = int.Parse(args[0]);
            }

            $"UDP server port: {port}".Info();
            
            // Create a new UDP echo server
            var server = new RelayServer(IPAddress.Any, port);

            // Start the server
            "Server starting...".Info();
            server.Start();
            "Server started".Info();

            "Press Enter to stop the server or '!' to restart the server...".Info();

            // Perform text input
            for (;;)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }

                // Restart the server
                if (line != "!")
                {
                    continue;
                }
                "Server restarting...".Info();
                server.Restart();
                "Server restarted".Info();
            }

            // Stop the server
            "Server stopping...".Info();
            server.Stop();
            "Server stopped, press any key to close".Info();
            Console.ReadKey(true);
        }
    }
}