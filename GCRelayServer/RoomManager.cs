using System.Collections.Concurrent;
using System.Net;
using Serilog;

namespace GCRelayServer;

public class RoomManager
{
    private readonly ConcurrentDictionary<uint, Room> rooms = new();

    public Room GetOrCreateRoom(uint matchingId, IPEndPoint endPoint)
    {
        var room = rooms.AddOrUpdate(
            matchingId,
            _ =>
            {
                var newRoom = new Room();
                newRoom.AddEndpoint(endPoint);
                return newRoom;
            },
            (_, existing) =>
            {
                if (existing.IsExpired)
                {
                    Log.Information("Room {MatchingId:X8} expired, resetting", matchingId);
                    var newRoom = new Room();
                    newRoom.AddEndpoint(endPoint);
                    return newRoom;
                }

                existing.AddEndpoint(endPoint);
                return existing;
            });

        return room;
    }

    public Room? TryGetRoom(uint matchingId)
    {
        if (!rooms.TryGetValue(matchingId, out var room))
        {
            Log.Warning("Room {MatchingId:X8} does not exist", matchingId);
            return null;
        }

        if (room.IsExpired)
        {
            Log.Warning("Room {MatchingId:X8} has expired", matchingId);
            rooms.TryRemove(matchingId, out _);
            return null;
        }

        return room;
    }

    public void EvictExpiredRooms()
    {
        var evicted = 0;
        foreach (var kvp in rooms)
        {
            if (kvp.Value.IsExpired && rooms.TryRemove(kvp.Key, out _))
            {
                evicted++;
            }
        }

        if (evicted > 0)
        {
            Log.Information("Evicted {Count} expired rooms", evicted);
        }
    }
}
