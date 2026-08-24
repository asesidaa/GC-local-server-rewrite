using System.Buffers.Binary;
using System.Net;
using System.Text;
using GCRelayServer;
using GCRelayServer.Protocol;
using GCRelayServer.Routing;

namespace MultiplayerTests.Relay;

public class RelayServerOptionsTests
{
    private static readonly DateTimeOffset Start = new(2026, 8, 25, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void NoArguments_UsesPort3333TenMinuteExpiryAndFiveSecondCleanup()
    {
        var parsed = RelayServerOptions.TryParse([], out var options, out var error);

        Assert.True(parsed, error);
        Assert.Equal(3333, options!.Port);
        Assert.Equal(TimeSpan.FromMinutes(10), options.RoomExpiry);
        Assert.Equal(TimeSpan.FromSeconds(5), options.CleanupInterval);
    }

    [Fact]
    public void PositionalPort_RemainsSupported()
    {
        var parsed = RelayServerOptions.TryParse(["4444"], out var options, out var error);

        Assert.True(parsed, error);
        Assert.Equal(4444, options!.Port);
    }

    [Fact]
    public void NamedOptions_OverridePortExpiryAndCleanup()
    {
        var parsed = RelayServerOptions.TryParse(
            ["--port", "5555", "--room-expiry-seconds", "120", "--cleanup-interval-seconds", "2"],
            out var options,
            out var error);

        Assert.True(parsed, error);
        Assert.Equal(5555, options!.Port);
        Assert.Equal(TimeSpan.FromSeconds(120), options.RoomExpiry);
        Assert.Equal(TimeSpan.FromSeconds(2), options.CleanupInterval);
    }

    [Theory]
    [MemberData(nameof(InvalidArguments))]
    public void InvalidPortDurationOrSyntax_IsRejected(string[] arguments)
    {
        var parsed = RelayServerOptions.TryParse(arguments, out var options, out var error);

        Assert.False(parsed);
        Assert.Null(options);
        Assert.False(string.IsNullOrWhiteSpace(error));
    }

    [Fact]
    public void CleanupWorker_RunOnceEvictsExpiredRooms()
    {
        var clock = new ManualTimeProvider(Start);
        var registry = new RelayRoomRegistry(clock, TimeSpan.FromSeconds(10));
        registry.Register(
            new IPEndPoint(IPAddress.Loopback, 40001),
            Announcement(1001, 0, "CARD000000000001"));
        var worker = new RelayCleanupWorker(registry, TimeSpan.FromSeconds(5), clock);
        clock.Advance(TimeSpan.FromSeconds(10));

        var evicted = worker.RunOnce();

        Assert.Equal(1, evicted);
        Assert.Equal(0, registry.RoomCount);
    }

    public static TheoryData<string[]> InvalidArguments()
    {
        return new TheoryData<string[]>
        {
            new[] { "0" },
            new[] { "65536" },
            new[] { "not-a-port" },
            new[] { "--port" },
            new[] { "--port", "0" },
            new[] { "--room-expiry-seconds", "0" },
            new[] { "--cleanup-interval-seconds", "-1" },
            new[] { "--unknown", "1" },
            new[] { "4444", "--port", "5555" }
        };
    }

    private static RelayDatagram Announcement(uint machineId, uint entry, string cardId)
    {
        var profile = new byte[256];
        BinaryPrimitives.WriteUInt32BigEndian(profile.AsSpan(0x0C, 4), 7);
        Encoding.ASCII.GetBytes(cardId).CopyTo(profile, 0x30);
        return new RelayDatagram(
            RelayMessageType.Announcement,
            4,
            0,
            101,
            entry,
            machineId,
            0,
            profile);
    }
}
