using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using BinarySerialization;
using Swan.Logging;

namespace GCRelayServer;

public class RelayServer : NetCoreServer.UdpServer
{
    private ConcurrentDictionary<uint, DictEntry> matchingDictionary = new();
    public RelayServer(IPAddress address, int port) : base(address, port) {}
    

    protected override void OnStarted()
    {
        // Start receive datagrams
        ReceiveAsync();
    }

    protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
    {
        var serializer = new BinarySerializer();
        var inputStream = new MemoryStream(buffer, (int)offset, (int)size, false);
        var packet = serializer.Deserialize<RelayPacket>(inputStream);

        if (!IsValidPacket(packet))
        {
            "Received malformed packet!".Warn();
            return;
        }
        
        $"Received packet from {endpoint}, type is 0x{packet.RequestSubType:X2}".Info();
        
        switch (packet.RequestSubType)
        {
            case RelayPacketTypes.HEART_BEAT:
            {
                for (var i = 0; i < 2; i++)
                {
                    SendPacketSingle(endpoint, packet, RelayPacketTypes.HEART_BEAT_RESPONSE);
                }
                
                break;
            }
            case RelayPacketTypes.START_MATCHING:
            {
                AddEntry(packet.MatchingId, endpoint);
                for (var i = 0; i < 2; i++)
                { 
                    SendPacketSingle(endpoint, packet, RelayPacketTypes.START_MATCHING_RESPONSE);
                }
                break;
            }
            case RelayPacketTypes.REGISTER_MATCHING:
            {
                var entry = AddEntry(packet.MatchingId, endpoint);
                SendPacketToOthers(entry.EndPoints, packet, endpoint);
                break;
            }
            default:
            {
                var entry = GetEntry(packet.MatchingId);
                if (entry is null)
                {
                    break;
                }
                SendPacketToOthers(entry.EndPoints, packet, endpoint);
                break;
            }
        }
        ReceiveAsync();
    }
    private void SendPacketSingle(EndPoint endpoint, RelayPacket packet, ushort subType)
    {
        packet.RequestSubType = subType;
        var serializer = new BinarySerializer();
        var sendStream = new MemoryStream(1024);
        serializer.Serialize(sendStream, packet);
        
        $"Send packet to {endpoint}, type is 0x{packet.RequestSubType:X2}".Info();
        SendAsync(endpoint, sendStream.GetBuffer(), 0, sendStream.Length);
    }

    private void SendPacketToOthers(IEnumerable<EndPoint> endPoints, RelayPacket packet, EndPoint owner)
    {
        if (owner is not IPEndPoint ipEndPoint)
        {
            "Endpoint is not IP endpoint! This should not happen!".Fatal();
            throw new ApplicationException();
        }

        foreach (var endPoint in endPoints.Where(endPoint => !ipEndPoint.Equals(endPoint)))
        {
            SendPacketSingle(endPoint, packet, packet.RequestSubType);
        }
    }

    protected override void OnError(SocketError error)
    {
        $"Relay server caught an error with code {error}".Error();
    }

    private static bool IsValidPacket(RelayPacket packet)
    {
        var totalSize = packet.Magic + packet.RemainingSize;
        var actualSize = 36 + packet.Data.Length;
        if (totalSize != actualSize)
        {
            return false;
        }

        return packet.Data.Length == packet.DataSize;
    }

    private DictEntry AddEntry(uint matchingId, EndPoint endPoint)
    {
        var now = DateTime.Now;
        var entry = matchingDictionary.GetValueOrDefault(matchingId, new DictEntry());
        var shouldClear = false;

        if (entry.LastAccessTime <= now && now - entry.LastAccessTime >= TimeSpan.FromMinutes(10))
        {
            $"Entry for matching id {matchingId:X8} has expired! Clients will be cleared!".Info();
            shouldClear = true;
        }
        if (entry.EndPoints.Count >= 4)
        {
            $"Entry for matching id {matchingId:X8} contains more than 4 clients! Clients will be cleared!".Warn();
            shouldClear = true;
        }
        entry.AddEndpoint(endPoint, shouldClear);
        
        entry.LastAccessTime = DateTime.Now;
        matchingDictionary[matchingId] = entry;

        return entry;
    }

    private DictEntry? GetEntry(uint matchingId)
    {
        var now = DateTime.Now;

        if (!matchingDictionary.ContainsKey(matchingId))    
        {
            $"Entry for matching id {matchingId:X8} does not exist!".Warn();
            return null;
        }

        var entry = matchingDictionary[matchingId];
        if (entry.LastAccessTime <= now && now - entry.LastAccessTime >= TimeSpan.FromMinutes(10))
        {
            $"Entry for matching id {matchingId:X8} has expired!".Warn();
            return null;
        }
        
        entry.LastAccessTime = DateTime.Now;
        return entry;
    }
}