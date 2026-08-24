namespace GCRelayServer.Protocol;

public sealed class RelayDatagram
{
    public const ushort ExpectedHeaderSize = 12;
    public const ushort RelayChannel = 6;
    public const int RoutedHeaderSize = 36;
    public const int MaxDatagramSize = 8192;

    private readonly byte[] payload;

    public RelayDatagram(
        RelayMessageType messageType,
        ushort mode,
        ushort routeParameter,
        uint matchingId,
        uint entryNumber,
        uint senderMachineId,
        uint targetMachineId,
        ReadOnlySpan<byte> payload)
    {
        MessageType = messageType;
        Mode = mode;
        RouteParameter = routeParameter;
        MatchingId = matchingId;
        EntryNumber = entryNumber;
        SenderMachineId = senderMachineId;
        TargetMachineId = targetMachineId;
        this.payload = payload.ToArray();
    }

    public ushort HeaderSize => ExpectedHeaderSize;

    public ushort Channel => RelayChannel;

    public RelayMessageType MessageType { get; }

    public ushort Mode { get; }

    public ushort RouteParameter { get; }

    public uint MatchingId { get; }

    public uint EntryNumber { get; }

    public uint SenderMachineId { get; }

    public uint TargetMachineId { get; }

    public ushort PayloadSize => checked((ushort)payload.Length);

    public ReadOnlyMemory<byte> Payload => payload;

    public RelayDatagram WithMessageType(RelayMessageType messageType, ushort? mode = null)
    {
        return new RelayDatagram(
            messageType,
            mode ?? Mode,
            RouteParameter,
            MatchingId,
            EntryNumber,
            SenderMachineId,
            TargetMachineId,
            payload);
    }

    public RelayDatagram WithTarget(uint targetMachineId)
    {
        return new RelayDatagram(
            MessageType,
            Mode,
            RouteParameter,
            MatchingId,
            EntryNumber,
            SenderMachineId,
            targetMachineId,
            payload);
    }
}
