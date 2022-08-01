using System.Net;

namespace GCRelayServer;

public class DictEntry
{
    public List<EndPoint> EndPoints { get; set; } = new();
    
    public DateTime LastAccessTime { get; set; } = DateTime.Now;

    public void AddEndpoint(EndPoint endPoint, bool shouldClear = false)
    {
        if (shouldClear)
        {
            EndPoints.Clear();
        }
        if (EndPoints.Contains(endPoint))
        {
            return;
        }
        EndPoints.Add(endPoint);
    }
}