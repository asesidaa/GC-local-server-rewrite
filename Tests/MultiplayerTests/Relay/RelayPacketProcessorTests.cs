using System.Buffers.Binary;
using System.Net;
using GCRelayServer.Protocol;
using GCRelayServer.Routing;

namespace MultiplayerTests.Relay;

public class RelayPacketProcessorTests
{
    [Fact]
    public void HeartbeatBytes_DecodeRouteAndEncodeOneB1()
    {
        var processor = CreateProcessor();
        var source = new IPEndPoint(IPAddress.Loopback, 40001);
        var bytes = RelayDatagramCodec.Encode(new RelayDatagram(
            RelayMessageType.HeartbeatRequest,
            0,
            0,
            0,
            0,
            1001,
            0,
            new byte[8]));

        var result = processor.Process(source, bytes);

        Assert.Null(result.Rejection);
        Assert.Equal(RelayMessageType.HeartbeatRequest, result.Datagram!.MessageType);
        Assert.Equal(1001u, result.Datagram.SenderMachineId);
        Assert.Equal(RelayRouteDisposition.HeartbeatAcknowledged, result.Disposition);
        var transmission = Assert.Single(result.Transmissions);
        Assert.Equal(source, transmission.Target);
        Assert.Equal(0x00B1, BinaryPrimitives.ReadUInt16BigEndian(transmission.Bytes.AsSpan(0x0C, 2)));
        Assert.Equal(44, transmission.Bytes.Length);
    }

    [Fact]
    public void MalformedBytes_ReturnBoundedRejectionWithoutTransmission()
    {
        var processor = CreateProcessor();

        var result = processor.Process(new IPEndPoint(IPAddress.Loopback, 40001), new byte[20]);

        Assert.Equal(RelayRejectionCode.DatagramTooShort, result.Rejection!.Value.Code);
        Assert.Null(result.Datagram);
        Assert.Null(result.Disposition);
        Assert.Empty(result.Transmissions);
    }

    private static RelayPacketProcessor CreateProcessor()
    {
        var clock = new ManualTimeProvider(new DateTimeOffset(2026, 8, 25, 0, 0, 0, TimeSpan.Zero));
        var registry = new RelayRoomRegistry(clock, TimeSpan.FromMinutes(10));
        return new RelayPacketProcessor(new RelayRouter(registry));
    }
}
