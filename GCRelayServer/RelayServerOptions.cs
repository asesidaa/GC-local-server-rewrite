namespace GCRelayServer;

public sealed record RelayServerOptions(int Port, TimeSpan RoomExpiry, TimeSpan CleanupInterval)
{
    private const int DefaultPort = 3333;
    private const int DefaultRoomExpirySeconds = 600;
    private const int DefaultCleanupIntervalSeconds = 5;

    public static bool TryParse(
        IReadOnlyList<string> arguments,
        out RelayServerOptions? options,
        out string? error)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        var port = DefaultPort;
        var roomExpirySeconds = DefaultRoomExpirySeconds;
        var cleanupIntervalSeconds = DefaultCleanupIntervalSeconds;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var index = 0;

        if (arguments.Count > 0 && !arguments[0].StartsWith("--", StringComparison.Ordinal))
        {
            if (!TryParsePort(arguments[0], out port))
            {
                options = null;
                error = $"Invalid relay port '{arguments[0]}'; expected 1..65535.";
                return false;
            }

            seen.Add("--port");
            index++;
        }

        while (index < arguments.Count)
        {
            var name = arguments[index++];
            if (name is not ("--port" or "--room-expiry-seconds" or "--cleanup-interval-seconds"))
            {
                options = null;
                error = $"Unknown relay option '{name}'.";
                return false;
            }

            if (!seen.Add(name))
            {
                options = null;
                error = $"Relay option '{name}' was supplied more than once.";
                return false;
            }

            if (index >= arguments.Count)
            {
                options = null;
                error = $"Relay option '{name}' requires a value.";
                return false;
            }

            var value = arguments[index++];
            switch (name)
            {
                case "--port" when !TryParsePort(value, out port):
                    options = null;
                    error = $"Invalid relay port '{value}'; expected 1..65535.";
                    return false;

                case "--room-expiry-seconds" when !TryParsePositive(value, out roomExpirySeconds):
                    options = null;
                    error = $"Invalid room expiry '{value}'; expected a positive number of seconds.";
                    return false;

                case "--cleanup-interval-seconds" when !TryParsePositive(value, out cleanupIntervalSeconds):
                    options = null;
                    error = $"Invalid cleanup interval '{value}'; expected a positive number of seconds.";
                    return false;
            }
        }

        options = new RelayServerOptions(
            port,
            TimeSpan.FromSeconds(roomExpirySeconds),
            TimeSpan.FromSeconds(cleanupIntervalSeconds));
        error = null;
        return true;
    }

    private static bool TryParsePort(string value, out int port)
    {
        return int.TryParse(value, out port) && port is >= 1 and <= 65535;
    }

    private static bool TryParsePositive(string value, out int seconds)
    {
        return int.TryParse(value, out seconds) && seconds > 0;
    }
}
