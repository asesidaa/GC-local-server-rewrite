using System.Net;
using GCRelayServer.Protocol;

namespace GCRelayServer.Routing;

public sealed class RelayRoomRegistry
{
    private readonly Lock gate = new();
    private readonly Dictionary<uint, RelayRoom> rooms = [];
    private readonly TimeProvider timeProvider;
    private readonly TimeSpan idleExpiry;

    public RelayRoomRegistry(TimeProvider timeProvider, TimeSpan idleExpiry)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        if (idleExpiry <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(idleExpiry));
        }

        this.timeProvider = timeProvider;
        this.idleExpiry = idleExpiry;
    }

    public int RoomCount
    {
        get
        {
            lock (gate)
            {
                return rooms.Count;
            }
        }
    }

    public RelayRegistrationResult Register(IPEndPoint endpoint, RelayDatagram datagram)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(datagram);

        if (datagram.MatchingId == 0)
        {
            return RelayRegistrationResult.Rejected(new RelayRejection(
                RelayRejectionCode.InvalidMatchingId,
                "Membership requires a nonzero matching ID."));
        }

        if (datagram.EntryNumber >= 4)
        {
            return RelayRegistrationResult.Rejected(new RelayRejection(
                RelayRejectionCode.InvalidEntryNumber,
                "Membership entry must be in 0..3."));
        }

        if (datagram.SenderMachineId == 0)
        {
            return RelayRegistrationResult.Rejected(new RelayRejection(
                RelayRejectionCode.InvalidMachineId,
                "Membership requires a nonzero sender machine ID."));
        }

        if (!RelayProfileIdentity.TryParse(datagram.Payload.Span, out var profileIdentity))
        {
            return RelayRegistrationResult.Rejected(new RelayRejection(
                RelayRejectionCode.InvalidPayloadContract,
                "Membership requires a 256-byte participant profile."));
        }

        var now = timeProvider.GetUtcNow();
        lock (gate)
        {
            if (rooms.TryGetValue(datagram.MatchingId, out var existingRoom))
            {
                if (existingRoom.IsExpired(now, idleExpiry))
                {
                    rooms.Remove(datagram.MatchingId);
                }
                else
                {
                    return existingRoom.Register(endpoint, datagram, profileIdentity, now);
                }
            }

            var newRoom = new RelayRoom(datagram.MatchingId, now);
            var result = newRoom.Register(endpoint, datagram, profileIdentity, now);
            if (result.Outcome != RelayRegistrationOutcome.Rejected)
            {
                rooms.Add(datagram.MatchingId, newRoom);
            }

            return result;
        }
    }

    public RelayMembershipOperationResult Remove(IPEndPoint endpoint, RelayDatagram datagram)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(datagram);
        var now = timeProvider.GetUtcNow();

        lock (gate)
        {
            if (!rooms.TryGetValue(datagram.MatchingId, out var room) || room.IsExpired(now, idleExpiry))
            {
                rooms.Remove(datagram.MatchingId);
                return RelayMembershipOperationResult.Failure(new RelayRejection(
                    RelayRejectionCode.RoomNotFound,
                    $"Relay room {datagram.MatchingId} does not exist."));
            }

            var result = room.Remove(endpoint, datagram, now);
            if (result.IsSuccess && room.IsEmpty)
            {
                rooms.Remove(datagram.MatchingId);
            }

            return result;
        }
    }

    public bool TryGetRoom(uint matchingId, out RelayRoom? room)
    {
        var now = timeProvider.GetUtcNow();
        lock (gate)
        {
            if (!rooms.TryGetValue(matchingId, out room))
            {
                return false;
            }

            if (!room.IsExpired(now, idleExpiry))
            {
                return true;
            }

            rooms.Remove(matchingId);
            room = null;
            return false;
        }
    }

    public RelaySenderResolution ResolveSender(IPEndPoint endpoint, RelayDatagram datagram)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(datagram);
        var now = timeProvider.GetUtcNow();

        lock (gate)
        {
            if (!rooms.TryGetValue(datagram.MatchingId, out var room) || room.IsExpired(now, idleExpiry))
            {
                rooms.Remove(datagram.MatchingId);
                return RelaySenderResolution.Failure(new RelayRejection(
                    RelayRejectionCode.RoomNotFound,
                    $"Relay room {datagram.MatchingId} does not exist."));
            }

            return room.ResolveSender(endpoint, datagram, now);
        }
    }

    public int EvictExpiredRooms()
    {
        var now = timeProvider.GetUtcNow();
        var evicted = 0;
        lock (gate)
        {
            foreach (var (matchingId, room) in rooms.ToArray())
            {
                if (room.IsExpired(now, idleExpiry) && rooms.Remove(matchingId))
                {
                    evicted++;
                }
            }
        }

        return evicted;
    }
}
