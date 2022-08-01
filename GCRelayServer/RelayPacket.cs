using BinarySerialization;

namespace GCRelayServer;

public class RelayPacket
{
    [FieldOrder(0)]
    [FieldEndianness(Endianness.Big)]
    public ushort Magic;

    [FieldOrder(1)]
    [FieldEndianness(Endianness.Big)]
    public ushort RemainingSize;

    [FieldOrder(2)]
    [FieldEndianness(Endianness.Big)]
    public ushort RequestMainType;

    [FieldOrder(3)]
    [FieldCount(6)]
    public byte[] Unknown0 = Array.Empty<byte>();
    
    [FieldOrder(4)]
    [FieldEndianness(Endianness.Big)]
    public ushort RequestSubType;
    
    [FieldOrder(5)]
    [FieldEndianness(Endianness.Big)]
    public ushort Unknown1;
    
    [FieldOrder(6)]
    [FieldEndianness(Endianness.Big)]
    public ushort DataSize;

    [FieldOrder(7)]
    [FieldEndianness(Endianness.Big)]
    public ushort Unknown2;

    [FieldOrder(8)]
    [FieldEndianness(Endianness.Big)]
    public uint MatchingId;

    [FieldOrder(9)]
    [FieldEndianness(Endianness.Big)]
    public uint EntryNo;

    [FieldOrder(10)]
    [FieldEndianness(Endianness.Big)]
    public uint MachineId;

    [FieldOrder(11)]
    [FieldEndianness(Endianness.Big)]
    public uint Unknown3;

    [FieldOrder(12)]
    public byte[] Data = Array.Empty<byte>();
}