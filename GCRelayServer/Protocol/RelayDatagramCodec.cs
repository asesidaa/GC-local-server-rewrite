using System.Buffers.Binary;

namespace GCRelayServer.Protocol;

public static class RelayDatagramCodec
{
    public static bool TryDecodeClient(
        ReadOnlySpan<byte> source,
        out RelayDatagram? datagram,
        out RelayRejection rejection)
    {
        datagram = null;
        if (source.Length < RelayDatagram.RoutedHeaderSize)
        {
            rejection = new RelayRejection(
                RelayRejectionCode.DatagramTooShort,
                $"Received {source.Length} bytes; routed channel 6 requires at least 36.");
            return false;
        }

        if (source.Length > RelayDatagram.MaxDatagramSize)
        {
            rejection = new RelayRejection(
                RelayRejectionCode.DatagramTooLarge,
                $"Received {source.Length} bytes; native receive limit is 8192.");
            return false;
        }

        var headerSize = BinaryPrimitives.ReadUInt16BigEndian(source[0x00..0x02]);
        if (headerSize != RelayDatagram.ExpectedHeaderSize)
        {
            rejection = new RelayRejection(
                RelayRejectionCode.InvalidHeaderSize,
                $"Header size is {headerSize}; expected 12.");
            return false;
        }

        var remainingSize = BinaryPrimitives.ReadUInt16BigEndian(source[0x02..0x04]);
        if (remainingSize != source.Length - RelayDatagram.ExpectedHeaderSize)
        {
            rejection = new RelayRejection(
                RelayRejectionCode.InvalidRemainingSize,
                $"Remaining size is {remainingSize}; received length is {source.Length}.");
            return false;
        }

        var channel = BinaryPrimitives.ReadUInt16BigEndian(source[0x04..0x06]);
        if (channel != RelayDatagram.RelayChannel)
        {
            rejection = new RelayRejection(
                RelayRejectionCode.InvalidChannel,
                $"Channel {channel} is not public relay channel 6.");
            return false;
        }

        var channelParameter = BinaryPrimitives.ReadUInt16BigEndian(source[0x06..0x08]);
        var outerReserved = BinaryPrimitives.ReadUInt32BigEndian(source[0x08..0x0C]);
        if (channelParameter != 0 || outerReserved != 0)
        {
            rejection = new RelayRejection(
                RelayRejectionCode.InvalidOuterReserved,
                "Native channel-6 builders write zero outer reserved fields.");
            return false;
        }

        var payloadSize = BinaryPrimitives.ReadUInt16BigEndian(source[0x10..0x12]);
        if (source.Length != RelayDatagram.RoutedHeaderSize + payloadSize)
        {
            rejection = new RelayRejection(
                RelayRejectionCode.InvalidPayloadSize,
                $"Payload declares {payloadSize} bytes but datagram length is {source.Length}.");
            return false;
        }

        var rawMessageType = BinaryPrimitives.ReadUInt16BigEndian(source[0x0C..0x0E]);
        var messageType = (RelayMessageType)rawMessageType;
        if (!RelayProtocol.TryGetRule(messageType, out _))
        {
            rejection = new RelayRejection(
                RelayRejectionCode.UnknownMessage,
                $"Subtype 0x{rawMessageType:X4} is not in the native inventory.");
            return false;
        }

        datagram = new RelayDatagram(
            messageType,
            BinaryPrimitives.ReadUInt16BigEndian(source[0x0E..0x10]),
            BinaryPrimitives.ReadUInt16BigEndian(source[0x12..0x14]),
            BinaryPrimitives.ReadUInt32BigEndian(source[0x14..0x18]),
            BinaryPrimitives.ReadUInt32BigEndian(source[0x18..0x1C]),
            BinaryPrimitives.ReadUInt32BigEndian(source[0x1C..0x20]),
            BinaryPrimitives.ReadUInt32BigEndian(source[0x20..0x24]),
            source[0x24..]);

        if (!RelayProtocol.TryValidateClient(datagram, out rejection))
        {
            datagram = null;
            return false;
        }

        rejection = RelayRejection.None;
        return true;
    }

    public static byte[] Encode(RelayDatagram datagram)
    {
        ArgumentNullException.ThrowIfNull(datagram);
        if (!RelayProtocol.TryValidateForEncoding(datagram, out var rejection))
        {
            throw new ArgumentException(rejection.ToString(), nameof(datagram));
        }

        var output = new byte[RelayDatagram.RoutedHeaderSize + datagram.PayloadSize];
        BinaryPrimitives.WriteUInt16BigEndian(output.AsSpan(0x00, 2), RelayDatagram.ExpectedHeaderSize);
        BinaryPrimitives.WriteUInt16BigEndian(
            output.AsSpan(0x02, 2),
            checked((ushort)(24 + datagram.PayloadSize)));
        BinaryPrimitives.WriteUInt16BigEndian(output.AsSpan(0x04, 2), RelayDatagram.RelayChannel);
        BinaryPrimitives.WriteUInt16BigEndian(output.AsSpan(0x0C, 2), (ushort)datagram.MessageType);
        BinaryPrimitives.WriteUInt16BigEndian(output.AsSpan(0x0E, 2), datagram.Mode);
        BinaryPrimitives.WriteUInt16BigEndian(output.AsSpan(0x10, 2), datagram.PayloadSize);
        BinaryPrimitives.WriteUInt16BigEndian(output.AsSpan(0x12, 2), datagram.RouteParameter);
        BinaryPrimitives.WriteUInt32BigEndian(output.AsSpan(0x14, 4), datagram.MatchingId);
        BinaryPrimitives.WriteUInt32BigEndian(output.AsSpan(0x18, 4), datagram.EntryNumber);
        BinaryPrimitives.WriteUInt32BigEndian(output.AsSpan(0x1C, 4), datagram.SenderMachineId);
        BinaryPrimitives.WriteUInt32BigEndian(output.AsSpan(0x20, 4), datagram.TargetMachineId);
        datagram.Payload.Span.CopyTo(output.AsSpan(0x24));
        return output;
    }
}
