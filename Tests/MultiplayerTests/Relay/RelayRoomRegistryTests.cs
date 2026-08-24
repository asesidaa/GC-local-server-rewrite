using System.Buffers.Binary;
using System.Net;
using GCRelayServer.Protocol;
using GCRelayServer.Routing;

namespace MultiplayerTests.Relay;

public class RelayRoomRegistryTests
{
    private static readonly DateTimeOffset Start = new(2026, 8, 25, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void FirstMember_BindsMachineEntryEndpointAndProfileIdentity()
    {
        var fixture = CreateFixture();
        var endpoint = Endpoint(1);
        var datagram = Announcement(machineId: 1001, entryNumber: 0, eventId: 7, cardId: "CARD000000000001");

        var result = fixture.Registry.Register(endpoint, datagram);

        Assert.Equal(RelayRegistrationOutcome.Added, result.Outcome);
        Assert.Empty(result.ExistingMembers);
        Assert.Equal(1, fixture.Registry.RoomCount);
        var member = Assert.Single(GetRoom(fixture.Registry).GetMembers());
        Assert.Equal(1001u, member.MachineId);
        Assert.Equal(0u, member.EntryNumber);
        Assert.Equal(endpoint, member.Endpoint);
        Assert.Equal(7u, member.ProfileIdentity.EventId);
        Assert.Equal(Start, member.LastSeenAt);
    }

    [Fact]
    public void ExactReannouncement_RefreshesDynamicProfileAndActivity()
    {
        var fixture = CreateFixture();
        var endpoint = Endpoint(1);
        var first = Announcement(1001, 0, profile: Profile(7, "CARD000000000001", messageId: 10));
        fixture.Registry.Register(endpoint, first);
        fixture.Clock.Advance(TimeSpan.FromSeconds(5));
        var changed = Announcement(1001, 0, profile: Profile(7, "CARD000000000001", messageId: 99));

        var result = fixture.Registry.Register(endpoint, changed);

        Assert.Equal(RelayRegistrationOutcome.Refreshed, result.Outcome);
        var room = GetRoom(fixture.Registry);
        var member = Assert.Single(room.GetMembers());
        Assert.Equal(99u, BinaryPrimitives.ReadUInt32BigEndian(member.LatestProfile.Span[0x20..0x24]));
        Assert.Equal(Start.AddSeconds(5), member.LastSeenAt);
        Assert.Equal(Start.AddSeconds(5), room.LastActivityAt);
    }

    [Theory]
    [InlineData(1001, 1, 1, 7, "CARD000000000001", RelayRejectionCode.MemberConflict)]
    [InlineData(1001, 0, 2, 7, "CARD000000000001", RelayRejectionCode.SenderEndpointMismatch)]
    [InlineData(1001, 0, 1, 8, "CARD000000000001", RelayRejectionCode.MemberConflict)]
    [InlineData(1001, 0, 1, 7, "CARD000000000999", RelayRejectionCode.MemberConflict)]
    [InlineData(2002, 0, 2, 7, "CARD000000000002", RelayRejectionCode.MemberConflict)]
    public void ConflictingMachineEntryEndpointOrProfile_IsRejected(
        uint machineId,
        uint entryNumber,
        int endpointNumber,
        uint eventId,
        string cardId,
        RelayRejectionCode expected)
    {
        var fixture = CreateFixture();
        fixture.Registry.Register(
            Endpoint(1),
            Announcement(1001, 0, eventId: 7, cardId: "CARD000000000001"));

        var result = fixture.Registry.Register(
            Endpoint(endpointNumber),
            Announcement(machineId, entryNumber, eventId: eventId, cardId: cardId));

        Assert.Equal(RelayRegistrationOutcome.Rejected, result.Outcome);
        Assert.Equal(expected, result.Rejection!.Value.Code);
        Assert.Single(GetRoom(fixture.Registry).GetMembers());
    }

    [Fact]
    public void FifthMember_IsRejectedWithoutClearingExistingMembers()
    {
        var fixture = CreateFixture();
        for (uint entry = 0; entry < 4; entry++)
        {
            fixture.Registry.Register(
                Endpoint(checked((int)entry + 1)),
                Announcement(1001 + entry, entry, cardId: $"CARD{entry:D12}"));
        }

        var fifth = fixture.Registry.Register(
            Endpoint(5),
            Announcement(1005, 3, cardId: "CARD000000000005"));

        Assert.Equal(RelayRegistrationOutcome.Rejected, fifth.Outcome);
        Assert.Equal(RelayRejectionCode.RoomFull, fifth.Rejection!.Value.Code);
        Assert.Equal([1001u, 1002u, 1003u, 1004u],
            GetRoom(fixture.Registry).GetMembers().Select(member => member.MachineId).Order());
    }

    [Fact]
    public void RemovingLastMember_RemovesRoom()
    {
        var fixture = CreateFixture();
        var endpoint = Endpoint(1);
        fixture.Registry.Register(endpoint, Announcement(1001, 0));

        var removed = fixture.Registry.Remove(endpoint, Announcement(1001, 0, type: RelayMessageType.Leave));

        Assert.True(removed.IsSuccess);
        Assert.Equal(0, fixture.Registry.RoomCount);
    }

    [Fact]
    public void EvictExpiredRooms_UsesInjectedTimeProvider()
    {
        var fixture = CreateFixture(TimeSpan.FromMinutes(10));
        fixture.Registry.Register(Endpoint(1), Announcement(1001, 0));
        fixture.Clock.Advance(TimeSpan.FromSeconds(599));

        var early = fixture.Registry.EvictExpiredRooms();
        fixture.Clock.Advance(TimeSpan.FromSeconds(1));
        var expired = fixture.Registry.EvictExpiredRooms();

        Assert.Equal(0, early);
        Assert.Equal(1, expired);
        Assert.Equal(0, fixture.Registry.RoomCount);
    }

    [Fact]
    public void Registration_RejectsProfileWithoutEventIdentity()
    {
        var fixture = CreateFixture();

        var result = fixture.Registry.Register(
            Endpoint(1),
            Announcement(1001, 0, eventId: 0, cardId: "CARD000000000001"));

        Assert.Equal(RelayRegistrationOutcome.Rejected, result.Outcome);
        Assert.Equal(RelayRejectionCode.InvalidPayloadContract, result.Rejection!.Value.Code);
        Assert.Equal(0, fixture.Registry.RoomCount);
    }

    private static Fixture CreateFixture(TimeSpan? expiry = null)
    {
        var clock = new ManualTimeProvider(Start);
        return new Fixture(new RelayRoomRegistry(clock, expiry ?? TimeSpan.FromMinutes(10)), clock);
    }

    private static RelayDatagram Announcement(
        uint machineId,
        uint entryNumber,
        uint eventId = 7,
        string cardId = "CARD000000000001",
        byte[]? profile = null,
        RelayMessageType type = RelayMessageType.Announcement)
    {
        return new RelayDatagram(
            type,
            type == RelayMessageType.Announcement ? (ushort)4 : (ushort)0,
            0,
            101,
            entryNumber,
            machineId,
            0,
            type == RelayMessageType.Leave ? new byte[8] : profile ?? Profile(eventId, cardId, messageId: 10));
    }

    private static byte[] Profile(uint eventId, string cardId, uint messageId)
    {
        var profile = new byte[256];
        BinaryPrimitives.WriteUInt32BigEndian(profile.AsSpan(0x0C, 4), eventId);
        BinaryPrimitives.WriteUInt32BigEndian(profile.AsSpan(0x20, 4), messageId);
        var cardBytes = System.Text.Encoding.ASCII.GetBytes(cardId);
        Assert.True(cardBytes.Length <= 16);
        cardBytes.CopyTo(profile, 0x30);
        return profile;
    }

    private static IPEndPoint Endpoint(int number) => new(IPAddress.Loopback, 40000 + number);

    private static RelayRoom GetRoom(RelayRoomRegistry registry)
    {
        Assert.True(registry.TryGetRoom(101, out var room));
        return room!;
    }

    private sealed record Fixture(RelayRoomRegistry Registry, ManualTimeProvider Clock);
}
