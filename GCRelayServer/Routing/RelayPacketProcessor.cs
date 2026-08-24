using System.Net;
using GCRelayServer.Protocol;

namespace GCRelayServer.Routing;

public sealed record RelayEncodedTransmission(IPEndPoint Target, byte[] Bytes);

public sealed record RelayPacketProcessingResult(
    RelayDatagram? Datagram,
    RelayRouteDisposition? Disposition,
    RelayRejection? Rejection,
    IReadOnlyList<RelayEncodedTransmission> Transmissions);

public sealed class RelayPacketProcessor(RelayRouter router)
{
    public RelayPacketProcessingResult Process(IPEndPoint source, ReadOnlySpan<byte> bytes)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!RelayDatagramCodec.TryDecodeClient(bytes, out var datagram, out var decodeRejection))
        {
            return new RelayPacketProcessingResult(null, null, decodeRejection, []);
        }

        var route = router.Route(source, datagram!);
        if (route.Disposition == RelayRouteDisposition.Rejected)
        {
            return new RelayPacketProcessingResult(datagram, route.Disposition, route.Rejection, []);
        }

        var transmissions = route.Transmissions
            .Select(transmission => new RelayEncodedTransmission(
                transmission.Target,
                RelayDatagramCodec.Encode(transmission.Datagram)))
            .ToArray();
        return new RelayPacketProcessingResult(datagram, route.Disposition, null, transmissions);
    }
}
