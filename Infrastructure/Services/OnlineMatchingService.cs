using System.Collections.Concurrent;
using Application.Game.Card.OnlineMatching;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class OnlineMatchingService : IOnlineMatchingService
{
    private static readonly TimeSpan RoomExpiry = TimeSpan.FromMinutes(10);

    private readonly ConcurrentDictionary<long, MatchRoom> rooms = new();

    private long nextMatchId;

    private readonly ILogger<OnlineMatchingService> logger;

    public OnlineMatchingService(ILogger<OnlineMatchingService> logger)
    {
        this.logger = logger;
    }

    public MatchRoomSnapshot JoinOrCreateRoom(MatchEntry entry)
    {
        // Try to join an existing open room (atomic check-and-add inside TryAddEntry)
        foreach (var kvp in rooms)
        {
            var snapshot = kvp.Value.TryAddEntry(entry);
            if (snapshot is not null)
            {
                logger.LogInformation("Player {CardId} joined room {MatchId} ({Count} players)",
                    entry.CardId, kvp.Value.MatchId, snapshot.Entries.Count);
                return snapshot;
            }
        }

        // No open room found — create a new one
        var matchId = Interlocked.Increment(ref nextMatchId);
        var newRoom = new MatchRoom(matchId);
        var result = newRoom.TryAddEntry(entry)!; // always succeeds on a fresh room
        rooms[matchId] = newRoom;

        logger.LogInformation("Player {CardId} created new room {MatchId}", entry.CardId, matchId);
        return result;
    }

    public MatchRoomSnapshot? UpdateRoom(long matchId, long cardId, long messageId)
    {
        if (!rooms.TryGetValue(matchId, out var room))
        {
            logger.LogWarning("Room {MatchId} not found for update by card {CardId}", matchId, cardId);
            return null;
        }

        return room.Update(cardId, messageId);
    }

    public void CleanupStaleRooms()
    {
        var now = DateTime.UtcNow;
        var expiredCount = 0;

        foreach (var kvp in rooms)
        {
            if (now - kvp.Value.CreatedAt < RoomExpiry)
            {
                continue;
            }

            if (rooms.TryRemove(kvp.Key, out _))
            {
                expiredCount++;
            }
        }

        if (expiredCount > 0)
        {
            logger.LogInformation("Cleaned up {Count} expired match rooms", expiredCount);
        }
    }
}
