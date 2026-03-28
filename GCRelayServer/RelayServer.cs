using System.Net;
using System.Net.Sockets;
using BinarySerialization;
using Serilog;

namespace GCRelayServer;

public class RelayServer : NetCoreServer.UdpServer
{
    private readonly RoomManager roomManager;

    public RelayServer(IPAddress address, int port, RoomManager roomManager) : base(address, port)
    {
        this.roomManager = roomManager;
    }

    protected override void OnStarted()
    {
        ReceiveAsync();
    }

    protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
    {
        try
        {
            RelayPacket packet;
            try
            {
                var serializer = new BinarySerializer();
                var inputStream = new MemoryStream(buffer, (int)offset, (int)size, false);
                packet = serializer.Deserialize<RelayPacket>(inputStream);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to deserialize packet from {Endpoint} ({Size} bytes)", endpoint, size);
                return;
            }

            if (!IsValidPacket(packet))
            {
                Log.Warning("Received malformed packet from {Endpoint}", endpoint);
                return;
            }

            if (endpoint is not IPEndPoint ipEndPoint)
            {
                Log.Error("Endpoint is not an IPEndPoint — this should not happen");
                return;
            }

            Log.Debug("Received packet from {Endpoint}, type 0x{SubType:X2}, match {MatchingId:X8}",
                endpoint, packet.RequestSubType, packet.MatchingId);

            switch (packet.RequestSubType)
            {
                case RelayPacketTypes.HEART_BEAT:
                    HandleHeartbeat(ipEndPoint, packet);
                    break;

                case RelayPacketTypes.START_MATCHING:
                    HandleStartMatching(ipEndPoint, packet);
                    break;

                case RelayPacketTypes.REGISTER_MATCHING:
                    HandleRegisterMatching(ipEndPoint, packet);
                    break;

                default:
                    HandleRelay(ipEndPoint, packet);
                    break;
            }
        }
        finally
        {
            ReceiveAsync();
        }
    }

    private void HandleHeartbeat(IPEndPoint sender, RelayPacket packet)
    {
        SendPacket(sender, packet, RelayPacketTypes.HEART_BEAT_RESPONSE);
        SendPacket(sender, packet, RelayPacketTypes.HEART_BEAT_RESPONSE);
    }

    private void HandleStartMatching(IPEndPoint sender, RelayPacket packet)
    {
        roomManager.GetOrCreateRoom(packet.MatchingId, sender);
        SendPacket(sender, packet, RelayPacketTypes.START_MATCHING_RESPONSE);
        SendPacket(sender, packet, RelayPacketTypes.START_MATCHING_RESPONSE);
    }

    private void HandleRegisterMatching(IPEndPoint sender, RelayPacket packet)
    {
        var room = roomManager.GetOrCreateRoom(packet.MatchingId, sender);
        var others = room.GetEndpointsExcluding(sender);
        foreach (var other in others)
        {
            SendPacket(other, packet, packet.RequestSubType);
        }
    }

    private void HandleRelay(IPEndPoint sender, RelayPacket packet)
    {
        var room = roomManager.TryGetRoom(packet.MatchingId);
        if (room is null)
        {
            return;
        }

        var others = room.GetEndpointsExcluding(sender);
        foreach (var other in others)
        {
            SendPacket(other, packet, packet.RequestSubType);
        }
    }

    private void SendPacket(EndPoint target, RelayPacket packet, ushort subType)
    {
        // Serialize with the desired subType without mutating the shared packet object
        var originalSubType = packet.RequestSubType;
        packet.RequestSubType = subType;

        var serializer = new BinarySerializer();
        var sendStream = new MemoryStream(1024);
        serializer.Serialize(sendStream, packet);

        // Restore original to avoid corrupting concurrent readers
        packet.RequestSubType = originalSubType;

        Log.Debug("Sending packet to {Endpoint}, type 0x{SubType:X2}", target, subType);
        SendAsync(target, sendStream.GetBuffer(), 0, sendStream.Length);
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

    protected override void OnError(SocketError error)
    {
        Log.Error("Relay server socket error: {Error}", error);
        ReceiveAsync();
    }
}
