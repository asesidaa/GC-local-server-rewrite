using System.Buffers.Binary;
using GCRelayServer.Protocol;

namespace MultiplayerTests.Relay;

public class RelayDatagramCodecTests
{
    [Fact]
    public void HeartbeatFixture_DecodesEveryHeaderField()
    {
        var fixture = CreateFixture(
            RelayMessageType.HeartbeatRequest,
            mode: 0,
            payload: new byte[8],
            matchingId: 0x10203040,
            entryNumber: 2,
            senderMachineId: 0x55667788,
            targetMachineId: 0);

        var decoded = RelayDatagramCodec.TryDecodeClient(fixture, out var datagram, out var rejection);

        Assert.True(decoded, rejection.ToString());
        Assert.Equal(12, datagram!.HeaderSize);
        Assert.Equal(6, datagram.Channel);
        Assert.Equal(RelayMessageType.HeartbeatRequest, datagram.MessageType);
        Assert.Equal(0, datagram.Mode);
        Assert.Equal(8, datagram.PayloadSize);
        Assert.Equal(0x10203040u, datagram.MatchingId);
        Assert.Equal(2u, datagram.EntryNumber);
        Assert.Equal(0x55667788u, datagram.SenderMachineId);
        Assert.Equal(0u, datagram.TargetMachineId);
    }

    [Fact]
    public void AnnouncementFixture_CopiesProfileOutOfReceiveBuffer()
    {
        var profile = Enumerable.Range(0, 256).Select(value => (byte)value).ToArray();
        var fixture = CreateFixture(RelayMessageType.Announcement, 4, profile);

        var decoded = RelayDatagramCodec.TryDecodeClient(fixture, out var datagram, out var rejection);
        fixture[36] ^= 0xFF;

        Assert.True(decoded, rejection.ToString());
        Assert.Equal(profile, datagram!.Payload.ToArray());
    }

    [Fact]
    public void GameplayFixture_RoundTripsByteForByte()
    {
        var payload = new byte[]
        {
            0x00, 0x00, 0x04, 0xD2,
            0x00, 0x00, 0x00, 0x0F,
            0xFF, 0xFF, 0xFF, 0xFF
        };
        var fixture = CreateFixture(
            RelayMessageType.MusicSelection,
            2,
            payload,
            routeParameter: 4,
            matchingId: 0x11223344,
            entryNumber: 3,
            senderMachineId: 0x55667788,
            targetMachineId: 0xFFFFFFFF);

        Assert.True(RelayDatagramCodec.TryDecodeClient(fixture, out var datagram, out var rejection), rejection.ToString());

        Assert.Equal(fixture, RelayDatagramCodec.Encode(datagram!));
    }

    [Theory]
    [MemberData(nameof(StructuralRejections))]
    public void StructuralErrors_AreRejected(byte[] fixture, RelayRejectionCode expected)
    {
        var decoded = RelayDatagramCodec.TryDecodeClient(fixture, out var datagram, out var rejection);

        Assert.False(decoded);
        Assert.Null(datagram);
        Assert.Equal(expected, rejection.Code);
    }

    [Theory]
    [InlineData(RelayMessageType.RegistrationAck, 0, 256)]
    [InlineData(RelayMessageType.NewMember, 4, 256)]
    [InlineData(RelayMessageType.HeartbeatResponse, 0, 8)]
    public void RelayOutputTypes_AreRejectedFromClients(RelayMessageType type, ushort mode, int payloadSize)
    {
        var fixture = CreateFixture(type, mode, new byte[payloadSize]);

        var decoded = RelayDatagramCodec.TryDecodeClient(fixture, out _, out var rejection);

        Assert.False(decoded);
        Assert.Equal(RelayRejectionCode.OutputOnlyMessage, rejection.Code);
    }

    [Theory]
    [MemberData(nameof(ContractRejections))]
    public void InvalidModeOrPayloadContract_IsRejected(
        RelayMessageType type,
        ushort mode,
        byte[] payload,
        RelayRejectionCode expected)
    {
        var fixture = CreateFixture(type, mode, payload);

        var decoded = RelayDatagramCodec.TryDecodeClient(fixture, out _, out var rejection);

        Assert.False(decoded);
        Assert.Equal(expected, rejection.Code);
    }

    [Fact]
    public void Encoder_WritesFreshRelayResponseWithoutClientDirectionCheck()
    {
        var response = new RelayDatagram(
            RelayMessageType.HeartbeatResponse,
            mode: 0,
            routeParameter: 0,
            matchingId: 0,
            entryNumber: 0,
            senderMachineId: 1001,
            targetMachineId: 0,
            new byte[8]);

        var encoded = RelayDatagramCodec.Encode(response);

        Assert.Equal(44, encoded.Length);
        Assert.Equal(0x00B1, BinaryPrimitives.ReadUInt16BigEndian(encoded.AsSpan(0x0C, 2)));
        Assert.Equal(8, BinaryPrimitives.ReadUInt16BigEndian(encoded.AsSpan(0x10, 2)));
    }

    public static TheoryData<byte[], RelayRejectionCode> StructuralRejections()
    {
        var tooShort = new byte[35];
        var tooLarge = new byte[8193];
        var invalidHeader = CreateFixture(RelayMessageType.HeartbeatRequest, 0, new byte[8]);
        BinaryPrimitives.WriteUInt16BigEndian(invalidHeader.AsSpan(0, 2), 11);
        var invalidRemaining = CreateFixture(RelayMessageType.HeartbeatRequest, 0, new byte[8]);
        BinaryPrimitives.WriteUInt16BigEndian(invalidRemaining.AsSpan(2, 2), 31);
        var invalidChannel = CreateFixture(RelayMessageType.HeartbeatRequest, 0, new byte[8]);
        BinaryPrimitives.WriteUInt16BigEndian(invalidChannel.AsSpan(4, 2), 5);
        var invalidChannelParameter = CreateFixture(RelayMessageType.HeartbeatRequest, 0, new byte[8]);
        BinaryPrimitives.WriteUInt16BigEndian(invalidChannelParameter.AsSpan(6, 2), 1);
        var invalidReserved = CreateFixture(RelayMessageType.HeartbeatRequest, 0, new byte[8]);
        BinaryPrimitives.WriteUInt32BigEndian(invalidReserved.AsSpan(8, 4), 1);
        var invalidPayloadSize = CreateFixture(RelayMessageType.HeartbeatRequest, 0, new byte[8]);
        BinaryPrimitives.WriteUInt16BigEndian(invalidPayloadSize.AsSpan(0x10, 2), 7);
        var unknownSubtype = CreateFixture((RelayMessageType)0x2222, 0, new byte[8]);

        return new TheoryData<byte[], RelayRejectionCode>
        {
            { tooShort, RelayRejectionCode.DatagramTooShort },
            { tooLarge, RelayRejectionCode.DatagramTooLarge },
            { invalidHeader, RelayRejectionCode.InvalidHeaderSize },
            { invalidRemaining, RelayRejectionCode.InvalidRemainingSize },
            { invalidChannel, RelayRejectionCode.InvalidChannel },
            { invalidChannelParameter, RelayRejectionCode.InvalidOuterReserved },
            { invalidReserved, RelayRejectionCode.InvalidOuterReserved },
            { invalidPayloadSize, RelayRejectionCode.InvalidPayloadSize },
            { unknownSubtype, RelayRejectionCode.UnknownMessage }
        };
    }

    public static TheoryData<RelayMessageType, ushort, byte[], RelayRejectionCode> ContractRejections()
    {
        var nonzeroHeartbeat = new byte[8];
        nonzeroHeartbeat[0] = 1;
        return new TheoryData<RelayMessageType, ushort, byte[], RelayRejectionCode>
        {
            { RelayMessageType.PhaseBarrier, 3, new byte[4], RelayRejectionCode.InvalidMode },
            { RelayMessageType.MusicSelection, 4, new byte[12], RelayRejectionCode.InvalidMode },
            { RelayMessageType.MusicSelectionCommit, 4, new byte[8], RelayRejectionCode.InvalidPayloadContract },
            { RelayMessageType.LiveTuneSnapshot, 1, new byte[24], RelayRejectionCode.InvalidPayloadContract },
            { RelayMessageType.LiveTuneSnapshot, 2, new byte[84], RelayRejectionCode.InvalidPayloadContract },
            { RelayMessageType.HeartbeatRequest, 0, nonzeroHeartbeat, RelayRejectionCode.NonZeroReservedPayload }
        };
    }

    private static byte[] CreateFixture(
        RelayMessageType type,
        ushort mode,
        byte[] payload,
        ushort routeParameter = 0,
        uint matchingId = 0x10203040,
        uint entryNumber = 0,
        uint senderMachineId = 1001,
        uint targetMachineId = 0)
    {
        var datagram = new byte[36 + payload.Length];
        BinaryPrimitives.WriteUInt16BigEndian(datagram.AsSpan(0x00, 2), 12);
        BinaryPrimitives.WriteUInt16BigEndian(datagram.AsSpan(0x02, 2), checked((ushort)(24 + payload.Length)));
        BinaryPrimitives.WriteUInt16BigEndian(datagram.AsSpan(0x04, 2), 6);
        BinaryPrimitives.WriteUInt16BigEndian(datagram.AsSpan(0x0C, 2), (ushort)type);
        BinaryPrimitives.WriteUInt16BigEndian(datagram.AsSpan(0x0E, 2), mode);
        BinaryPrimitives.WriteUInt16BigEndian(datagram.AsSpan(0x10, 2), checked((ushort)payload.Length));
        BinaryPrimitives.WriteUInt16BigEndian(datagram.AsSpan(0x12, 2), routeParameter);
        BinaryPrimitives.WriteUInt32BigEndian(datagram.AsSpan(0x14, 4), matchingId);
        BinaryPrimitives.WriteUInt32BigEndian(datagram.AsSpan(0x18, 4), entryNumber);
        BinaryPrimitives.WriteUInt32BigEndian(datagram.AsSpan(0x1C, 4), senderMachineId);
        BinaryPrimitives.WriteUInt32BigEndian(datagram.AsSpan(0x20, 4), targetMachineId);
        payload.CopyTo(datagram, 36);
        return datagram;
    }
}
