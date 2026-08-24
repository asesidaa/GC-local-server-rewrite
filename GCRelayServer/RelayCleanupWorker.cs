using GCRelayServer.Routing;
using Serilog;

namespace GCRelayServer;

public sealed class RelayCleanupWorker
{
    private readonly RelayRoomRegistry registry;
    private readonly TimeSpan interval;
    private readonly TimeProvider timeProvider;

    public RelayCleanupWorker(RelayRoomRegistry registry, TimeSpan interval, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(timeProvider);
        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(interval));
        }

        this.registry = registry;
        this.interval = interval;
        this.timeProvider = timeProvider;
    }

    public int RunOnce()
    {
        var evicted = registry.EvictExpiredRooms();
        if (evicted > 0)
        {
            Log.Information("Evicted {Count} expired relay rooms", evicted);
        }

        return evicted;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(interval, timeProvider);
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                RunOnce();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Normal process shutdown.
        }
    }
}
