using System.Net;

namespace GCRelayServer.Routing;

public sealed class RelayMember
{
    private byte[] latestProfile;

    internal RelayMember(
        uint machineId,
        uint entryNumber,
        IPEndPoint endpoint,
        RelayProfileIdentity profileIdentity,
        ReadOnlySpan<byte> profile,
        DateTimeOffset now)
    {
        MachineId = machineId;
        EntryNumber = entryNumber;
        Endpoint = CopyEndpoint(endpoint);
        ProfileIdentity = profileIdentity;
        latestProfile = profile.ToArray();
        LastSeenAt = now;
    }

    public uint MachineId { get; }

    public uint EntryNumber { get; }

    public IPEndPoint Endpoint { get; }

    public RelayProfileIdentity ProfileIdentity { get; }

    public ReadOnlyMemory<byte> LatestProfile => latestProfile;

    public DateTimeOffset LastSeenAt { get; private set; }

    internal bool IsEndpoint(IPEndPoint endpoint) => Endpoint.Equals(endpoint);

    internal void Refresh(ReadOnlySpan<byte> profile, DateTimeOffset now)
    {
        latestProfile = profile.ToArray();
        LastSeenAt = now;
    }

    private static IPEndPoint CopyEndpoint(IPEndPoint endpoint) => new(endpoint.Address, endpoint.Port);
}
