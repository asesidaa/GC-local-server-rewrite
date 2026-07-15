using System.Buffers.Binary;
using System.Text;

namespace DatParser;

public sealed class BigEndianBinaryReader(Stream stream) : IDisposable
{
    private readonly BinaryReader _reader = new(stream, Encoding.UTF8, leaveOpen: false);

    public ushort ReadUInt16() => BinaryPrimitives.ReverseEndianness(_reader.ReadUInt16());

    public uint ReadUInt32() => BinaryPrimitives.ReverseEndianness(_reader.ReadUInt32());

    public byte ReadByte() => _reader.ReadByte();

    public byte[] ReadBytes(int count) => _reader.ReadBytes(count);

    public string ReadLengthPrefixedString()
    {
        var len = _reader.ReadByte();
        if (len == 0) return string.Empty;
        var bytes = _reader.ReadBytes(len);
        return Encoding.UTF8.GetString(bytes);
    }

    public void Skip(int count) => _reader.ReadBytes(count);

    public void Dispose() => _reader.Dispose();
}
