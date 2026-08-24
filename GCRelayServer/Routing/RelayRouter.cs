using System.Net;
using GCRelayServer.Protocol;

namespace GCRelayServer.Routing;

public sealed class RelayRouter(RelayRoomRegistry registry)
{
    public RelayRouteResult Route(IPEndPoint source, RelayDatagram datagram)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(datagram);

        if (!RelayProtocol.TryValidateClient(datagram, out var rejection))
        {
            return RelayRouteResult.Rejected(rejection);
        }

        return datagram.MessageType switch
        {
            RelayMessageType.HeartbeatRequest => AcknowledgeHeartbeat(source, datagram),
            RelayMessageType.Announcement => HandleAnnouncement(source, datagram),
            RelayMessageType.Registration => HandleRegistration(source, datagram),
            RelayMessageType.Leave => HandleLeave(source, datagram),
            RelayMessageType.Start => RouteStart(source, datagram),
            _ => RelayRouteResult.Rejected(new RelayRejection(
                RelayRejectionCode.InvalidTarget,
                $"Routing for {datagram.MessageType} is not active."))
        };
    }

    private static RelayRouteResult AcknowledgeHeartbeat(IPEndPoint source, RelayDatagram datagram)
    {
        var response = datagram.WithMessageType(RelayMessageType.HeartbeatResponse, mode: 0);
        return RelayRouteResult.Accepted(
            RelayRouteDisposition.HeartbeatAcknowledged,
            [new RelayTransmission(source, response)]);
    }

    private RelayRouteResult HandleAnnouncement(IPEndPoint source, RelayDatagram datagram)
    {
        var registration = registry.Register(source, datagram);
        if (registration.Outcome == RelayRegistrationOutcome.Rejected)
        {
            return RelayRouteResult.Rejected(registration.Rejection!.Value);
        }

        if (registration.Outcome == RelayRegistrationOutcome.Added)
        {
            var newMember = datagram.WithMessageType(RelayMessageType.NewMember, mode: 4);
            return RelayRouteResult.Accepted(
                RelayRouteDisposition.MemberAdded,
                registration.ExistingMembers
                    .Select(member => new RelayTransmission(member.Endpoint, newMember))
                    .ToArray());
        }

        return RelayRouteResult.Accepted(
            RelayRouteDisposition.MemberRefreshed,
            registration.ExistingMembers
                .Select(member => new RelayTransmission(member.Endpoint, datagram))
                .ToArray());
    }

    private RelayRouteResult HandleRegistration(IPEndPoint source, RelayDatagram datagram)
    {
        var registration = registry.Register(source, datagram);
        if (registration.Outcome == RelayRegistrationOutcome.Rejected)
        {
            return RelayRouteResult.Rejected(registration.Rejection!.Value);
        }

        var transmissions = new List<RelayTransmission>();
        if (registration.Outcome == RelayRegistrationOutcome.Added)
        {
            var newMember = datagram.WithMessageType(RelayMessageType.NewMember, mode: 4);
            transmissions.AddRange(registration.ExistingMembers.Select(
                member => new RelayTransmission(member.Endpoint, newMember)));
        }

        transmissions.Add(new RelayTransmission(
            source,
            datagram.WithMessageType(RelayMessageType.RegistrationAck, mode: 0)));
        return RelayRouteResult.Accepted(
            registration.Outcome == RelayRegistrationOutcome.Added
                ? RelayRouteDisposition.MemberAddedAndAcknowledged
                : RelayRouteDisposition.MemberAcknowledged,
            transmissions);
    }

    private RelayRouteResult HandleLeave(IPEndPoint source, RelayDatagram datagram)
    {
        var removal = registry.Remove(source, datagram);
        return removal.IsSuccess
            ? RelayRouteResult.Accepted(RelayRouteDisposition.MemberRemoved)
            : RelayRouteResult.Rejected(removal.Rejection!.Value);
    }

    private RelayRouteResult RouteStart(IPEndPoint source, RelayDatagram datagram)
    {
        var sender = registry.ResolveSender(source, datagram);
        if (!sender.IsSuccess)
        {
            return RelayRouteResult.Rejected(sender.Rejection!.Value);
        }

        if (sender.Sender!.EntryNumber != 0)
        {
            return RelayRouteResult.Rejected(new RelayRejection(
                RelayRejectionCode.SenderRoleMismatch,
                "Only stable entry zero may send the host start message."));
        }

        return RelayRouteResult.Accepted(
            RelayRouteDisposition.Routed,
            sender.OtherMembers
                .Select(member => new RelayTransmission(member.Endpoint, datagram))
                .ToArray());
    }
}
