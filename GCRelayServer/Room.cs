using System.Net;

namespace GCRelayServer;

public class Room
{
    private static readonly TimeSpan ExpiryDuration = TimeSpan.FromMinutes(10);
    private const int MaxEndpoints = 4;

    private readonly Lock _lock = new();
    private readonly List<IPEndPoint> endpoints = [];

    public DateTime LastAccessTime { get; private set; } = DateTime.UtcNow;

    public bool IsExpired => DateTime.UtcNow - LastAccessTime >= ExpiryDuration;

    public bool AddEndpoint(IPEndPoint endPoint)
    {
        lock (_lock)
        {
            LastAccessTime = DateTime.UtcNow;

            if (endpoints.Count >= MaxEndpoints)
            {
                endpoints.Clear();
            }

            if (endpoints.Contains(endPoint))
            {
                return false;
            }

            endpoints.Add(endPoint);
            return true;
        }
    }

    public IPEndPoint[] GetEndpointsExcluding(IPEndPoint sender)
    {
        lock (_lock)
        {
            LastAccessTime = DateTime.UtcNow;
            return endpoints.Where(ep => !ep.Equals(sender)).ToArray();
        }
    }

    public IPEndPoint[] GetAllEndpoints()
    {
        lock (_lock)
        {
            return [.. endpoints];
        }
    }
}
