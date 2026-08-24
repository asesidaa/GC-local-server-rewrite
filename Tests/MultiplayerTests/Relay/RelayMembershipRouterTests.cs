using System.Buffers.Binary;
using System.Net;
using System.Text;
using GCRelayServer.Protocol;
using GCRelayServer.Routing;

namespace MultiplayerTests.Relay;

public class RelayMembershipRouterTests
{
    private static readonly DateTimeOffset Start = new(2026, 8, 25, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Heartbeat_EmitsOneB1WithoutCreatingRoom()
    {
        var fixture = CreateFixture();
        var request = Datagram(RelayMessageType.HeartbeatRequest, 1001, 0, new byte[8], mode: 0, matchingId: 0);

        var result = fixture.Router.Route(Endpoint(1), request);

        Assert.Equal(RelayRouteDisposition.HeartbeatAcknowledged, result.Disposition);
        var transmission = Assert.Single(result.Transmissions);
        Assert.Equal(Endpoint(1), transmission.Target);
        Assert.Equal(RelayMessageType.HeartbeatResponse, transmission.Datagram.MessageType);
        Assert.Equal(0, fixture.Registry.RoomCount);
    }

    [Fact]
    public void FirstA6_RegistersWithoutOutboundTransmission()
    {
        var fixture = CreateFixture();

        var result = fixture.Router.Route(Endpoint(1), Announcement(1001, 0, "CARD000000000001"));

        Assert.Equal(RelayRouteDisposition.MemberAdded, result.Disposition);
        Assert.Empty(result.Transmissions);
        Assert.Equal(1, fixture.Registry.RoomCount);
    }

    [Fact]
    public void NewA6_EmitsA2OnlyToExistingMembers()
    {
        var fixture = CreateFixture();
        fixture.Router.Route(Endpoint(1), Announcement(1001, 0, "CARD000000000001"));
        var second = Announcement(1002, 1, "CARD000000000002");

        var result = fixture.Router.Route(Endpoint(2), second);

        Assert.Equal(RelayRouteDisposition.MemberAdded, result.Disposition);
        var transmission = Assert.Single(result.Transmissions);
        Assert.Equal(Endpoint(1), transmission.Target);
        Assert.Equal(RelayMessageType.NewMember, transmission.Datagram.MessageType);
        Assert.Equal(4, transmission.Datagram.Mode);
        Assert.Equal(1002u, transmission.Datagram.SenderMachineId);
        Assert.Equal(second.Payload.ToArray(), transmission.Datagram.Payload.ToArray());
    }

    [Fact]
    public void KnownA6_RemainsA6AndReachesEveryOtherMember()
    {
        var fixture = CreateFixture();
        fixture.Router.Route(Endpoint(1), Announcement(1001, 0, "CARD000000000001"));
        fixture.Router.Route(Endpoint(2), Announcement(1002, 1, "CARD000000000002"));
        fixture.Router.Route(Endpoint(3), Announcement(1003, 2, "CARD000000000003"));
        var reannouncement = Announcement(1001, 0, "CARD000000000001", messageId: 99);

        var result = fixture.Router.Route(Endpoint(1), reannouncement);

        Assert.Equal(RelayRouteDisposition.MemberRefreshed, result.Disposition);
        Assert.Equal([Endpoint(2), Endpoint(3)], result.Transmissions.Select(item => item.Target).OrderBy(item => item.Port));
        Assert.All(result.Transmissions, transmission =>
            Assert.Equal(RelayMessageType.Announcement, transmission.Datagram.MessageType));
    }

    [Fact]
    public void LostA6_A0FallbackRegistersSendsA2ThenOneA1()
    {
        var fixture = CreateFixture();
        fixture.Router.Route(Endpoint(1), Announcement(1001, 0, "CARD000000000001"));
        var registration = Registration(1002, 1, "CARD000000000002");

        var result = fixture.Router.Route(Endpoint(2), registration);

        Assert.Equal(RelayRouteDisposition.MemberAddedAndAcknowledged, result.Disposition);
        Assert.Equal(2, result.Transmissions.Count);
        Assert.Equal(Endpoint(1), result.Transmissions[0].Target);
        Assert.Equal(RelayMessageType.NewMember, result.Transmissions[0].Datagram.MessageType);
        Assert.Equal(4, result.Transmissions[0].Datagram.Mode);
        Assert.Equal(Endpoint(2), result.Transmissions[1].Target);
        Assert.Equal(RelayMessageType.RegistrationAck, result.Transmissions[1].Datagram.MessageType);
        Assert.Equal(0, result.Transmissions[1].Datagram.Mode);
    }

    [Fact]
    public void KnownA0_EmitsOneA1ToSender()
    {
        var fixture = CreateFixture();
        fixture.Router.Route(Endpoint(1), Announcement(1001, 0, "CARD000000000001"));

        var result = fixture.Router.Route(Endpoint(1), Registration(1001, 0, "CARD000000000001"));

        Assert.Equal(RelayRouteDisposition.MemberAcknowledged, result.Disposition);
        var transmission = Assert.Single(result.Transmissions);
        Assert.Equal(Endpoint(1), transmission.Target);
        Assert.Equal(RelayMessageType.RegistrationAck, transmission.Datagram.MessageType);
    }

    [Fact]
    public void A3_RemovesAndIsNotForwarded()
    {
        var fixture = CreateFixture();
        fixture.Router.Route(Endpoint(1), Announcement(1001, 0, "CARD000000000001"));
        fixture.Router.Route(Endpoint(2), Announcement(1002, 1, "CARD000000000002"));

        var result = fixture.Router.Route(Endpoint(2), Leave(1002, 1));

        Assert.Equal(RelayRouteDisposition.MemberRemoved, result.Disposition);
        Assert.Empty(result.Transmissions);
        Assert.True(fixture.Registry.TryGetRoom(101, out var room));
        Assert.Equal(1001u, Assert.Single(room!.GetMembers()).MachineId);
    }

    [Fact]
    public void A7_RequiresEntryZeroAndBroadcastsToOthers()
    {
        var fixture = CreateFixture();
        fixture.Router.Route(Endpoint(1), Announcement(1001, 0, "CARD000000000001"));
        fixture.Router.Route(Endpoint(2), Announcement(1002, 1, "CARD000000000002"));

        var host = fixture.Router.Route(Endpoint(1), StartMessage(1001, 0));
        var guest = fixture.Router.Route(Endpoint(2), StartMessage(1002, 1));

        Assert.Equal(RelayRouteDisposition.Routed, host.Disposition);
        Assert.Equal(Endpoint(2), Assert.Single(host.Transmissions).Target);
        Assert.Equal(RelayRouteDisposition.Rejected, guest.Disposition);
        Assert.Equal(RelayRejectionCode.SenderRoleMismatch, guest.Rejection!.Value.Code);
    }

    private static Fixture CreateFixture()
    {
        var clock = new ManualTimeProvider(Start);
        var registry = new RelayRoomRegistry(clock, TimeSpan.FromMinutes(10));
        return new Fixture(new RelayRouter(registry), registry);
    }

    private static RelayDatagram Announcement(uint machineId, uint entry, string cardId, uint messageId = 10)
    {
        return Datagram(RelayMessageType.Announcement, machineId, entry, Profile(7, cardId, messageId), mode: 4);
    }

    private static RelayDatagram Registration(uint machineId, uint entry, string cardId)
    {
        return Datagram(RelayMessageType.Registration, machineId, entry, Profile(7, cardId, 10), mode: 0);
    }

    private static RelayDatagram Leave(uint machineId, uint entry)
    {
        return Datagram(RelayMessageType.Leave, machineId, entry, new byte[8], mode: 0);
    }

    private static RelayDatagram StartMessage(uint machineId, uint entry)
    {
        return Datagram(RelayMessageType.Start, machineId, entry, new byte[8], mode: 1);
    }

    private static RelayDatagram Datagram(
        RelayMessageType type,
        uint machineId,
        uint entry,
        byte[] payload,
        ushort mode,
        uint matchingId = 101,
        uint target = 0)
    {
        return new RelayDatagram(type, mode, 0, matchingId, entry, machineId, target, payload);
    }

    private static byte[] Profile(uint eventId, string cardId, uint messageId)
    {
        var profile = new byte[256];
        BinaryPrimitives.WriteUInt32BigEndian(profile.AsSpan(0x0C, 4), eventId);
        BinaryPrimitives.WriteUInt32BigEndian(profile.AsSpan(0x20, 4), messageId);
        Encoding.ASCII.GetBytes(cardId).CopyTo(profile, 0x30);
        return profile;
    }

    private static IPEndPoint Endpoint(int number) => new(IPAddress.Loopback, 40000 + number);

    private sealed record Fixture(RelayRouter Router, RelayRoomRegistry Registry);
}
