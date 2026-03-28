using Application.Game.Card.OnlineMatching;

namespace Application.Interfaces;

public interface IOnlineMatchingService
{
    MatchRoomSnapshot JoinOrCreateRoom(MatchEntry entry);

    MatchRoomSnapshot? UpdateRoom(long matchId, long cardId, long messageId);

    void CleanupStaleRooms();
}
