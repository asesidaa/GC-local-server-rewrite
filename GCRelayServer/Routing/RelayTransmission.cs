using System.Net;
using GCRelayServer.Protocol;

namespace GCRelayServer.Routing;

public sealed record RelayTransmission(IPEndPoint Target, RelayDatagram Datagram);

public enum RelayRouteDisposition
{
    Rejected,
    HeartbeatAcknowledged,
    MemberAdded,
    MemberRefreshed,
    MemberAddedAndAcknowledged,
    MemberAcknowledged,
    MemberRemoved,
    Routed
}

public sealed record RelayRouteResult(
    RelayRouteDisposition Disposition,
    RelayRejection? Rejection,
    IReadOnlyList<RelayTransmission> Transmissions)
{
    public static RelayRouteResult Accepted(
        RelayRouteDisposition disposition,
        IReadOnlyList<RelayTransmission>? transmissions = null) =>
        new(disposition, null, transmissions ?? []);

    public static RelayRouteResult Rejected(RelayRejection rejection) =>
        new(RelayRouteDisposition.Rejected, rejection, []);
}
