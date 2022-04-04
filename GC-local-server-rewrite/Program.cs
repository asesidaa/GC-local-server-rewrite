using GCLocalServerRewrite.common;
using GCLocalServerRewrite.server;
using SQLitePCL;
using Swan;
using Swan.Logging;

namespace GCLocalServerRewrite;

internal class Program
{
    private static void Main(string[] args)
    {
        Batteries_V2.Init();

        InitializeLogging();

        var urlPrefixes = args.Length > 0 ? args : new[] { "http://*:80", "https://*:443" };

        using (var cts = new CancellationTokenSource())
        {
            Task.WaitAll(
                RunWebServerAsync(urlPrefixes, cts.Token),
                Task.CompletedTask,
                WaitForUserBreakAsync(cts.Cancel));
        }

        // Clean up
        "Bye".Info(nameof(Program));
        Terminal.Flush();

        Console.WriteLine("Press any key to exit.");
        WaitForKeypress();
    }

    private static void InitializeLogging()
    {
        if (!Directory.Exists(PathHelper.LogRootPath))
        {
            Directory.CreateDirectory(PathHelper.LogRootPath);
        }

        Logger.RegisterLogger(new FileLogger(PathHelper.LogRootPath, true));
    }


    /// <summary>
    ///     Create and run a web server.
    /// </summary>
    /// <param name="urlPrefixes"></param>
    /// <param name="cancellationToken"></param>
    private static async Task RunWebServerAsync(string[] urlPrefixes, CancellationToken cancellationToken)
    {
        using var server = Server.CreateWebServer(urlPrefixes);
        await server.RunAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Prompt the user to press any key;
    ///     when a key is next pressed, call the specified action to cancel operations.
    /// </summary>
    /// <param name="cancel"> Cancel Action to call </param>
    private static async Task WaitForUserBreakAsync(Action cancel)
    {
        // Be sure to run in parallel.
        await Task.Yield();

        "Press any key to stop the web server.".Info(nameof(Program));
        WaitForKeypress();
        "Stopping...".Info(nameof(Program));
        cancel();
    }

    /// <summary>
    ///     Clear the console input buffer and wait for a keypress
    /// </summary>
    private static void WaitForKeypress()
    {
        while (Console.KeyAvailable)
        {
            Console.ReadKey(true);
        }

        Console.ReadKey(true);
    }
}