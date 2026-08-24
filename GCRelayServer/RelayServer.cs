using System.Net;
using System.Net.Sockets;
using GCRelayServer.Routing;
using Serilog;

namespace GCRelayServer;

public sealed class RelayServer : NetCoreServer.UdpServer
{
    private readonly RelayPacketProcessor processor;

    public RelayServer(IPAddress address, int port, RelayPacketProcessor processor) : base(address, port)
    {
        this.processor = processor;
    }

    protected override void OnStarted()
    {
        ReceiveAsync();
    }

    protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
    {
        try
        {
            if (endpoint is not IPEndPoint source)
            {
                Log.Warning("Rejected relay datagram from non-IP endpoint {Endpoint}", endpoint);
                return;
            }

            if (offset < 0 || size < 0 || offset > int.MaxValue || size > int.MaxValue ||
                offset + size > buffer.LongLength)
            {
                Log.Warning(
                    "Rejected relay receive bounds from {Endpoint}: offset {Offset}, size {Size}, buffer {BufferSize}",
                    endpoint,
                    offset,
                    size,
                    buffer.LongLength);
                return;
            }

            var result = processor.Process(source, buffer.AsSpan((int)offset, (int)size));
            if (result.Rejection is { } rejection)
            {
                Log.Warning(
                    "Rejected relay datagram from {Endpoint}: {Code} ({Detail})",
                    source,
                    rejection.Code,
                    rejection.Detail);
                return;
            }

            var datagram = result.Datagram!;
            Log.Debug(
                "Relay {Disposition}: type 0x{MessageType:X4}, match {MatchingId:X8}, machine {MachineId:X8}, " +
                "entry {EntryNumber}, source {Endpoint}, targets {TargetCount}",
                result.Disposition,
                (ushort)datagram.MessageType,
                datagram.MatchingId,
                datagram.SenderMachineId,
                datagram.EntryNumber,
                source,
                result.Transmissions.Count);

            foreach (var transmission in result.Transmissions)
            {
                SendAsync(transmission.Target, transmission.Bytes, 0, transmission.Bytes.Length);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled relay receive failure from {Endpoint}", endpoint);
        }
        finally
        {
            ReceiveAsync();
        }
    }

    protected override void OnError(SocketError error)
    {
        Log.Error("Relay server socket error: {Error}", error);
        ReceiveAsync();
    }
}
