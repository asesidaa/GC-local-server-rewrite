using System.Buffers.Binary;
using System.Net;
using System.Text;
using GCRelayServer.Protocol;
using GCRelayServer.Routing;

namespace MultiplayerTests.Relay;

public class RelayGameplayRouterTests
{
    private static readonly DateTimeOffset Start = new(2026, 8, 25, 0, 0, 0, TimeSpan.Zero);

    [Theory]
    [MemberData(nameof(ValidGameplayMessages))]
    public void EveryClientGameplaySubtype_BroadcastsToOtherMembers(
        RelayMessageType type,
        ushort mode,
        int payloadSize,
        uint senderEntry)
    {
        var fixture = CreateFixture();
        RegisterHostAndGuest(fixture);
        var machineId = senderEntry == 0 ? 1001u : 1002u;
        var source = senderEntry == 0 ? Endpoint(1) : Endpoint(2);
        var expectedTarget = senderEntry == 0 ? Endpoint(2) : Endpoint(1);
        var payload = new byte[payloadSize];
        var datagram = Datagram(type, mode, machineId, senderEntry, payload);

        var result = fixture.Router.Route(source, datagram);

        Assert.Equal(RelayRouteDisposition.Routed, result.Disposition);
        var transmission = Assert.Single(result.Transmissions);
        Assert.Equal(expectedTarget, transmission.Target);
        Assert.Same(datagram, transmission.Datagram);
        Assert.Equal(type, transmission.Datagram.MessageType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4294967295)]
    public void BroadcastTargets_ExcludeSender(uint target)
    {
        var fixture = CreateFixture();
        RegisterThree(fixture);

        var result = fixture.Router.Route(
            Endpoint(1),
            Datagram(RelayMessageType.PhaseBarrier, 4, 1001, 0, new byte[4], target));

        Assert.Equal([Endpoint(2), Endpoint(3)], result.Transmissions.Select(item => item.Target).OrderBy(item => item.Port));
    }

    [Fact]
    public void ConcreteTarget_ReachesOnlyThatMachine()
    {
        var fixture = CreateFixture();
        RegisterThree(fixture);

        var result = fixture.Router.Route(
            Endpoint(1),
            Datagram(RelayMessageType.PhaseBarrier, 4, 1001, 0, new byte[4], target: 1003));

        Assert.Equal(Endpoint(3), Assert.Single(result.Transmissions).Target);
    }

    [Fact]
    public void UnknownConcreteTarget_IsRejected()
    {
        var fixture = CreateFixture();
        RegisterHostAndGuest(fixture);

        var result = fixture.Router.Route(
            Endpoint(1),
            Datagram(RelayMessageType.PhaseBarrier, 4, 1001, 0, new byte[4], target: 9999));

        Assert.Equal(RelayRouteDisposition.Rejected, result.Disposition);
        Assert.Equal(RelayRejectionCode.TargetNotFound, result.Rejection!.Value.Code);
    }

    [Theory]
    [InlineData(1001, 0, 1, RelayMessageType.MusicSelection, 2, RelayRejectionCode.SenderRoleMismatch)]
    [InlineData(1002, 1, 2, RelayMessageType.MusicSelection, 1, RelayRejectionCode.SenderRoleMismatch)]
    [InlineData(9999, 2, 3, RelayMessageType.PhaseBarrier, 4, RelayRejectionCode.SenderNotRegistered)]
    [InlineData(1001, 1, 1, RelayMessageType.PhaseBarrier, 4, RelayRejectionCode.SenderNotRegistered)]
    [InlineData(1001, 0, 3, RelayMessageType.PhaseBarrier, 4, RelayRejectionCode.SenderEndpointMismatch)]
    [InlineData(1002, 1, 1, RelayMessageType.PhaseBarrier, 4, RelayRejectionCode.SenderEndpointMismatch)]
    public void RoleAndSenderSpoofing_AreRejected(
        uint machineId,
        uint entry,
        int endpointNumber,
        RelayMessageType type,
        ushort mode,
        RelayRejectionCode expected)
    {
        var fixture = CreateFixture();
        RegisterHostAndGuest(fixture);
        var payloadSize = type == RelayMessageType.MusicSelection ? 12 : 4;

        var result = fixture.Router.Route(
            Endpoint(endpointNumber),
            Datagram(type, mode, machineId, entry, new byte[payloadSize]));

        Assert.Equal(RelayRouteDisposition.Rejected, result.Disposition);
        Assert.Equal(expected, result.Rejection!.Value.Code);
    }

    [Fact]
    public void RejectedRole_DoesNotRefreshRoomOrMemberActivity()
    {
        var fixture = CreateFixture();
        RegisterHostAndGuest(fixture);
        Assert.True(fixture.Registry.TryGetRoom(101, out var room));
        var guest = room!.GetMembers().Single(member => member.EntryNumber == 1);
        fixture.Clock.Advance(TimeSpan.FromSeconds(9));

        var result = fixture.Router.Route(
            Endpoint(2),
            Datagram(RelayMessageType.HostStatus, 1, 1002, 1, new byte[16]));

        Assert.Equal(RelayRouteDisposition.Rejected, result.Disposition);
        Assert.Equal(Start, room.LastActivityAt);
        Assert.Equal(Start, guest.LastSeenAt);
    }

    public static TheoryData<RelayMessageType, ushort, int, uint> ValidGameplayMessages()
    {
        return new TheoryData<RelayMessageType, ushort, int, uint>
        {
            { RelayMessageType.PhaseBarrier, 4, 4, 1 },
            { RelayMessageType.MusicSelection, 1, 12, 0 },
            { RelayMessageType.MusicSelection, 2, 12, 1 },
            { RelayMessageType.MusicSelectionCommit, 4, 12, 1 },
            { RelayMessageType.DifficultyAndItem, 4, 8, 1 },
            { RelayMessageType.LiveTuneSnapshot, 1, 84, 0 },
            { RelayMessageType.LiveTuneSnapshot, 2, 24, 1 },
            { RelayMessageType.ResultRecord, 4, 88, 1 },
            { RelayMessageType.PresetMessage, 4, 4, 1 },
            { RelayMessageType.GuestLiveness, 2, 8, 1 },
            { RelayMessageType.HostStatus, 1, 16, 0 }
        };
    }

    private static Fixture CreateFixture()
    {
        var clock = new ManualTimeProvider(Start);
        var registry = new RelayRoomRegistry(clock, TimeSpan.FromMinutes(10));
        return new Fixture(new RelayRouter(registry), registry, clock);
    }

    private static void RegisterHostAndGuest(Fixture fixture)
    {
        fixture.Router.Route(Endpoint(1), Announcement(1001, 0, "CARD000000000001"));
        fixture.Router.Route(Endpoint(2), Announcement(1002, 1, "CARD000000000002"));
    }

    private static void RegisterThree(Fixture fixture)
    {
        RegisterHostAndGuest(fixture);
        fixture.Router.Route(Endpoint(3), Announcement(1003, 2, "CARD000000000003"));
    }

    private static RelayDatagram Announcement(uint machineId, uint entry, string cardId)
    {
        var profile = new byte[256];
        BinaryPrimitives.WriteUInt32BigEndian(profile.AsSpan(0x0C, 4), 7);
        Encoding.ASCII.GetBytes(cardId).CopyTo(profile, 0x30);
        return Datagram(RelayMessageType.Announcement, 4, machineId, entry, profile);
    }

    private static RelayDatagram Datagram(
        RelayMessageType type,
        ushort mode,
        uint machineId,
        uint entry,
        byte[] payload,
        uint target = 0)
    {
        return new RelayDatagram(type, mode, 0, 101, entry, machineId, target, payload);
    }

    private static IPEndPoint Endpoint(int number) => new(IPAddress.Loopback, 40000 + number);

    private sealed record Fixture(RelayRouter Router, RelayRoomRegistry Registry, ManualTimeProvider Clock);
}
