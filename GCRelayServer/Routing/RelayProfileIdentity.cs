using System.Buffers.Binary;

namespace GCRelayServer.Routing;

public readonly record struct RelayProfileIdentity(uint EventId, ulong CardIdHigh, ulong CardIdLow)
{
    public static bool TryParse(ReadOnlySpan<byte> profile, out RelayProfileIdentity identity)
    {
        if (profile.Length != 256)
        {
            identity = default;
            return false;
        }

        identity = new RelayProfileIdentity(
            BinaryPrimitives.ReadUInt32BigEndian(profile[0x0C..0x10]),
            BinaryPrimitives.ReadUInt64BigEndian(profile[0x30..0x38]),
            BinaryPrimitives.ReadUInt64BigEndian(profile[0x38..0x40]));
        return true;
    }
}
