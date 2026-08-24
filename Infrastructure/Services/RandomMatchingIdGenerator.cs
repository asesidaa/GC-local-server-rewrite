using System.Buffers.Binary;
using System.Security.Cryptography;
using Application.Interfaces;

namespace Infrastructure.Services;

public sealed class RandomMatchingIdGenerator : IMatchingIdGenerator
{
    public uint Next()
    {
        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        RandomNumberGenerator.Fill(bytes);
        return BinaryPrimitives.ReadUInt32LittleEndian(bytes);
    }
}
